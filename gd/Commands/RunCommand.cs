using GD.Services;
using GD.Services.Abstractions;
using GD.Utilities;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace GD.Commands;

internal class RunCommand : Command<RunCommand.RunSettings>
{
    private readonly IGDVersionResolver versionResolver;

    public class RunSettings : CommandSettings
    {
        [CommandArgument(0, "[version]")]
        [Description("Godot version to run. ")]
        public string Version { get; set; } = string.Empty;

        [CommandOption("--mono|-m", isRequired: false)]
        [Description("Use the .NET (Mono) build of Godot.")]
        public bool UseMono { get; set; } = false;

        [CommandOption("--console|-c", isRequired: false)]
        [Description("Run Godot via the console instead of launching the GUI.")]
        public bool UseConsole { get; set; } = false;

        //Other properties
        public bool HasVersion => Version != string.Empty;
    }

    public RunCommand(IGDVersionResolver versionResolver)
    {
        this.versionResolver = versionResolver;
    }
    public override int Execute(CommandContext context, RunSettings settings, CancellationToken cancellationToken)
    {
        if(context.Remaining.Parsed.Count > 0)
        {
            ConsoleMarkupUtility.PrintError("Too many arguments provided. Only the version argument is accepted.");
            return 1;
        }

        var version = versionResolver.ResolveVersion(settings.Version);

        if(version == null)
        {
            ConsoleMarkupUtility.PrintError($"The Godot version could not be resolved.");
            return 1;
        }

        var executablePath = versionResolver.ResolveVersionExecutablePath(version, settings.UseConsole, settings.UseMono);
        if (executablePath == null)
        {
            ConsoleMarkupUtility.PrintError($"The executable file path for the Godot version {version}{(settings.UseMono ? "(mono)" : "")} could not be found.");
            return -1;
        }

        GDGodotLauncher.LaunchGodot(executablePath);
        return 0;
    }
}