using GD.Infrastructure.Abstractions;
using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
namespace GD.Services;

internal class GDVersionResolver : IGDVersionResolver
{
    private const string GD_VERSION_FILE_NAME = ".godot-version";
    private const string GD_PROJECT_FILE_NAME = "project.godot";

    private readonly IGDConfigurationsFactory _configFactory;
    private readonly IGodotVersionsManager _versionManager;

    public GDVersionResolver(IGDConfigurationsFactory configs, IGodotVersionsManager versionManager)
    {
        _configFactory = configs;
        _versionManager = versionManager;
        //Load the configurations and the versions at the same time
        _versionManager.LoadVersions(_configFactory.ConfigurationsInstance);
    }
    public bool VersionExists(string version)
    {
        if (!_versionManager.DataLoaded) return false;
        return _versionManager.GodotVersions.Any(x => x.Version == version);
    }
    public string ResolveVersion(string version = null)
    {
        if (!_versionManager.DataLoaded) return null;
        //We need to determine the version
        // The rule is:
        //- 1. If version is provided, we return that instead
        //      a) if version only has major part, we select the latest version of the major part
        //  2. If version is not provided:
        //     a) First look at the root folder for the ".godot-version" file to select a version
        //     b) If a) does not work, look for the "project.godot" file to determine the project's godot version
        //     c) If b) does not work, default to the current godot version from the config class
        //     d) If c) does not work, default to the last downloaded version or latest version

        if (!string.IsNullOrEmpty(version))
        {
            if (!GodotVersion.IsValidVersionFormat(version, out string standardVersion))
            {
                ConsoleMarkupUtility.PrintError($"Invalid version number {version}");
                return null;
            }

            string[] parts = standardVersion.Split('.');

            //Check if the original version contained parts
            if (!version.Any(v => v == '.'))
            {
                //Only major part is provided, we select the latest version of the major part
                var latestVersion = _versionManager.GetLatestVersion(parts[0]);
                if (latestVersion != null)
                    return latestVersion.Version;
            }
            else if (version.Count(v => v == '.') == 1)
            {
                //Major and minor parts are provided, we select the latest version of the major and minor part
                var latestVersion = _versionManager.GetLatestVersion(parts[0], parts[1]);
                if (latestVersion != null)
                    return latestVersion.Version;
            }
            else
            {
                //Full version is provided, we check if it exists
                var v = _versionManager.GetByVersionString(standardVersion);
                if (v != null)
                    return v.Version;
            }
            //If we reach here, it means the provided version does not exist
            return null;
        }

        //from .godot-version file
        var finalVer = GetGodotVersionFromVersionFile();
        if (!string.IsNullOrEmpty(finalVer))
            return finalVer;

        //from project.godot file
        finalVer = GetGodotVersionFromProjectFile();
        if (!string.IsNullOrEmpty(finalVer))
            return finalVer;

        //default to the current godot version from the version manager
        if (_versionManager.GlobalGodotVersion != null)
            return _versionManager.GlobalGodotVersion.Version;

        var godotVersion = _versionManager.GetLatestVersion();
        if (godotVersion is not null)
            return godotVersion.Version;

        return null;
    }
    public string ResolveVersionExecutablePath(string version, bool useConsole = false, bool useMono = false)
    {
        if (!_versionManager.DataLoaded) return null;

        //No need to standardize the version, we assume that it's been standardized by the "Resolve"
        var godotVersion = _versionManager.GodotVersions.FirstOrDefault(x => x.Version == version && x.SupportsDotNet == useMono);

        if (godotVersion != null)
        {
            return useConsole ? godotVersion.ConsoleExecutablePath : godotVersion.GuiExecutablePath;
        }
        else
        {
            ConsoleMarkupUtility.PrintError($"Could not resolve the Godot version {version}.");
            return null;
        }
    }
    private static string GetGodotVersionFromProjectFile()
    {
        //NOTE: This will only work when using Godot v4.1+ since the feature was added from those version onward
        var project_file = Path.Combine(Directory.GetCurrentDirectory(), GD_PROJECT_FILE_NAME);
        if (File.Exists(project_file))
        {
            //Line to extract: config/features=PackedStringArray("4.3", "Forward Plus")
            var lines = File.ReadAllLines(project_file);
            var version_line = lines.FirstOrDefault(l => l.Trim().StartsWith("config/features=PackedStringArray"));
            if (version_line != null && version_line.Contains(','))
            {
                var version = version_line.Substring(0, version_line.IndexOf(','))
                                          .Replace("config/features=PackedStringArray", "")
                                          .Replace("\"", "")
                                          .Replace(" ", "");
                return version;
            }
        }
        return null;
    }
    private static string GetGodotVersionFromVersionFile()
    {
        var confFile = Path.Combine(Directory.GetCurrentDirectory(), GD_VERSION_FILE_NAME);
        if (File.Exists(confFile))
        {
            var versionLine = File.ReadAllLines(confFile)
                                  .Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x))
                                  .FirstOrDefault();
            return versionLine;
        }
        return null;
    }
}