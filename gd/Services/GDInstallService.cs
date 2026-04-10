using GD.Infrastructure.Abstractions;
using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
using Spectre.Console;
namespace GD.Services;

internal class GDInstallService : IGDInstallService
{
    private readonly IGDConfigurations configurations;
    private readonly IGodotVersionsManager versionManager;
    private readonly IGodotReleaseResolver releaseResolver;
    private GodotVersion godotVer;
    private ProgressTracker progressTracker;

    public GDInstallService(IGDConfigurationsFactory configFactory,
                            IGodotVersionsManager versionManager,
                            IGodotReleaseResolver releaseResolver)
    {
        this.versionManager = versionManager;
        this.releaseResolver = releaseResolver;

        //Create the configs and load versions
        this.configurations = configFactory.CreateConfigurations();
        versionManager.LoadVersions(this.configurations);
    }
    public void InstallGodot(string version, bool monoVersion, bool stable)
    {
        if(GodotVersion.IsValidVersionFormat(version, out var standardVersion))
        {
            //Compare the standard version with the versions already installed
            if(versionManager.GodotVersions.Any(x => x.Version == standardVersion && x.SupportsDotNet == monoVersion))
            {
                ConsoleMarkupUtility.PrintInfo($"The Godot version ${version}{(monoVersion ? "(mono)" : "")} has already been installed on your system.");
                return;
            }
        }
    }
    public async Task InstallGodot(GodotInstallParam param, CancellationToken token)
    {
        if (!versionManager.DataLoaded) return;

        if (string.IsNullOrEmpty(param.Platform))
        {
            //Deduce platform from the configurations
            param.Platform = configurations.GDConf.GetPlatformFor(param.MonoBuild);
        }

        godotVer = CreateGodotVersion(param);

        //Check if the version already exists
        if (versionManager.VersionExists(godotVer))
        {
            ConsoleMarkupUtility.PrintLine($"The Godot version already exists.");
            return;
        }

        ConsoleMarkupUtility.PrintLine($"Installing Godot version {godotVer.Version}-{param.Channel}{(godotVer.SupportsDotNet ? "(mono)" : "")}");

        var progressBar = AnsiConsole.Progress();
        progressBar.AutoClear = false;
        progressBar.Columns(
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            SpinnerColumnExtensions.CompletedText(new SpinnerColumn(Spinner.Known.DotsCircle), "Complete")
        );

        ServiceResult results = null;

        if(!FolderManager.MakeDirectory(godotVer.Path))
        {
            ConsoleMarkupUtility.PrintError($"Error installing Godot version {godotVer.Version}-{param.Channel}{(godotVer.SupportsDotNet ? "(mono)" : "")}");
            return;
        }

        await progressBar.StartAsync(async ctx =>
        {
            progressTracker = new(ctx);

            //results = await CompleteDownload(param, token);

            //if (!results.IsSuccess)return;

            //string tempDownloadPath = results.Result.ToString();

            string tempDownloadPath = "C:\\Users\\phiwo\\AppData\\Local\\gd\\temp\\Godot_v4.6.1-stable_mono_win64.zip";

            results = await CompleteExtraction(tempDownloadPath, token);
        });

        if(results is not null && !results.IsSuccess)
        {
            ConsoleMarkupUtility.PrintError($"Error installing Godot version {godotVer.Version}-{param.Channel}{(godotVer.SupportsDotNet ? "(mono)" : "")}");
            ConsoleMarkupUtility.HandleServiceResult(results);
            return;
        }

        //We need to capture the executables paths for the Godot version
        CaptureExecutablesPaths();


        //Now add to the version manager
        versionManager.AddVersion(godotVer);
        versionManager.SaveChanges(configurations);//Save the configurations
    }
    private void CaptureExecutablesPaths()
    {
        var dir = new DirectoryInfo(godotVer.Path);

        //Linux    --> no extension
        //MacOS    --> .app (this will be simply a folder and not an executable, but the OS will handle that)     
        //Windows  --> .exe

        //This filters only the executables for the executable files across all platforms
        List<FileInfo> files = [];

        switch (configurations.GDConf.OsType)
        {
            case OsType.Windows:
                files = [..dir.EnumerateFiles("*.exe")];
                break;
            case OsType.Linux:
                files = [..dir.EnumerateFiles().Where(x => string.IsNullOrEmpty(x.Extension))];
                break;
            case OsType.MacOS:
                files = [..dir.EnumerateFiles("*.app")];
                break;
        }

        if(files.Count == 0)
        {
            ConsoleMarkupUtility.PrintError($"No Godot executable could be found on the path `{godotVer.Path}`");
            return;
        }

        var consoleExecutable = files.FirstOrDefault(x => x.Name.Contains("console", StringComparison.OrdinalIgnoreCase))?.FullName ?? string.Empty;
        if(!string.IsNullOrEmpty(consoleExecutable))
        {
            //Ovveride the console file name
            godotVer.ConsoleExecutablePath = Path.Combine(godotVer.Path, Path.GetFileName(consoleExecutable));
            File.Move(consoleExecutable, godotVer.ConsoleExecutablePath);
        }

        //This removes the console build
        files = [..files.Where(x => !x.Name.Contains("console"))];
        if(files.Count == 0)
        {
            ConsoleMarkupUtility.PrintError("Could not find any GUI executable version of Godot.");
            return;
        }

        string guiExecutable;
        if (files.Count > 1)
        {
            var fileNames = files.Select(x => x.Name);
            ConsoleMarkupUtility.PrintWarning($"More than one executable was found for Godot Engine.");
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Select the [green]Godot executable[/]:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more executables)[/]")
                .AddChoices(fileNames));

            guiExecutable = files.First(x => x.Name == selection).FullName;
        }
        else
        {
            guiExecutable = files.First().FullName;
        }

        if (!string.IsNullOrEmpty(guiExecutable))
        {
            godotVer.GuiExecutablePath = Path.Combine(godotVer.Path, Path.GetFileName(guiExecutable));
            File.Move(guiExecutable, godotVer.GuiExecutablePath);
        }
    }
    private async Task<ServiceResult> CompleteDownload(GodotInstallParam param, CancellationToken token) 
    {
  
        //Start the task
        progressTracker.StartNewTaskToTrack(ProgressType.Downloading);
        string downloadFileName = $"Godot_v{param.GenerateTagName()}_{param.Platform}.zip";

        var gdRelease = await releaseResolver.GetReleaseAsset(param);

        if (gdRelease == null)
        {
            return ServiceResult.Fail("Could not find a corresponding release asset.");
        }

        string tempDownloadPath = Path.Combine(configurations.GDConf.TempDownloadFolder, downloadFileName);

        var results = await Downloader.DownloadFileAsync(gdRelease.BrowserDownloadUrl, tempDownloadPath, token, progressTracker.UpdateProgress);

        if (results.IsSuccess)
            return ServiceResult.Success(result: tempDownloadPath);

        return results;
    }
    private async Task<ServiceResult> CompleteExtraction(string tempDownloadPath, CancellationToken token)
    {
        var fileInfo = new FileInfo(tempDownloadPath);

        progressTracker.StartNewTaskToTrack(ProgressType.Extracting);

        var result = await FolderManager.ExtractZip(tempDownloadPath, godotVer.Path, token, progressTracker.UpdateProgress);

        if(result.IsSuccess)
        {
            //Clean up after the extraction
            fileInfo.Delete();
        }
        return result;
    }
    private GodotVersion CreateGodotVersion(GodotInstallParam parm)
    {
        var ver = new GodotVersion
        {
            Version = parm.Version,
            SupportsDotNet = parm.MonoBuild,
            Channel = parm.Channel,
            IsBroken = true,
            IsActive = false,
            IsStatableVersion = parm.Channel.Trim().Equals("stable", StringComparison.OrdinalIgnoreCase)
        };

        var config = configurations.GDConf;

        //Generate the paths and file names
        string folderName = GodotVersion.GenerateVersionFolderName(parm.Version, parm.MonoBuild);
        string guiFileName = GodotVersion.GenerateExecutableFileName(parm.Version, config.OsType, parm.MonoBuild, false);
        string consoleFileName = GodotVersion.GenerateExecutableFileName(parm.Version, config.OsType, parm.MonoBuild, true);

        string fullFolderPath = Path.Combine(config.InstallationsFolder, folderName);
        string fullGuiFileName = Path.Combine(fullFolderPath, guiFileName);
        string fullConsoleFileName = Path.Combine(fullFolderPath, consoleFileName);

        ver.Path = fullFolderPath;
        ver.GuiExecutablePath = fullGuiFileName;
        ver.ConsoleExecutablePath = fullConsoleFileName;

        return ver;
    }
}