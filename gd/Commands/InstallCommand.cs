using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace GD.Commands;

internal class InstallCommand : AsyncCommand<InstallCommand.InstallSettings>
{
    private readonly IGDInstallService _installService;
    public class InstallSettings : CommandSettings
    {
        [CommandArgument(0, "[version]")]
        [Description("Godot version to install. ")]
        public string Version { get; set; } = string.Empty;

        [CommandOption("--mono|--dotnet", isRequired: false)]
        [Description("Install the .NET (Mono) build of Godot.")]
        [DefaultValue(false)]
        public bool UseMono { get; set; } = false;

        [CommandOption("--stable", isRequired:false, IsHidden = true)]
        [Description("Install the stable Godot version.")]
        [DefaultValue(false)]
        public bool StableVersion { get; set; } = false;
        [CommandOption("--rc", isRequired:false, IsHidden =true)]
        [Description("Install the release candidate Godot version.")]
        [DefaultValue(false)]
        public bool RCVersion { get; set; } = false;
        [CommandOption("--dev", isRequired: false, IsHidden = true)]
        [Description("Install the nightly build of the Godot version.")]
        [DefaultValue(false)]
        public bool DevVersion { get; set; } = false;
        [CommandOption("--platform|-p", isRequired: false, IsHidden = true)]
        [Description("The system platform to download the version. If value is not provided, platform will be determined from your OS.")]
        public string Platform { get; set; } = string.Empty;
        [CommandOption("--channel | -c <channel>", isRequired:false)]
        public string Channel { get;set;  } = string.Empty;
        public bool IncludeTemplates { get; set;} = false;
        public bool HasVersion => Version != string.Empty;

    }
    public InstallCommand(IGDInstallService service)
    {
        _installService = service;
    }
    public override async Task<int> ExecuteAsync(CommandContext context, InstallSettings settings, CancellationToken cancellationToken)
    {
        if(!settings.HasVersion)
        {
            ConsoleMarkupUtility.PrintError("Missing version.");
            return 1;
        }
        if(context.Remaining.Parsed.Count > 0)
        {
            ConsoleMarkupUtility.PrintError("Too many arguments provided.");
            return 1;
        }
        //First try to resolve the command
        var installParam = BuildParameter(settings);
        //ConsoleMarkupUtility.PrintLine(JsonSerializer.Serialize(installParam));
        if(installParam != null)
        {
            //_installService.InstallGodot(settings.Version, settings.UseMono, settings.StableVersion);
            await _installService.InstallGodot(installParam, cancellationToken);
        }
        return 0;
    }
    private static GodotInstallParam BuildParameter(InstallSettings settings)
    {
        var param = new GodotInstallParam();
        param.Version = settings.Version;
        param.MonoBuild = settings.UseMono;
        //param.Channel
        if (string.IsNullOrEmpty(settings.Platform))
        {
            param.Platform = settings.Platform;
            param.PlatformProvidedByUser = true;
        }

        if(!string.IsNullOrEmpty(settings.Channel))
        {
            param.Channel = settings.Channel;
            return param;
        }

        bool[] channelsBooleans = [settings.RCVersion, settings.StableVersion, settings.DevVersion];
        if(!channelsBooleans.Any(x => x))
        {
            //By default, select the stable version
            param.Channel = "stable";
        }
        else if(channelsBooleans.Count(x => x) > 1)
        {
            ConsoleMarkupUtility.PrintError("Could not process more than one channel, choose atleast one channel.");
            return null;
        }
        else
        {
            param.Channel = settings.DevVersion ? "dev" :
                            settings.StableVersion ? "stable" :
                            settings.RCVersion ? "rc" :
                            "rc";
        }

        return param;
    }
}