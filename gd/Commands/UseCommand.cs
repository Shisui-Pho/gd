using GD.Utilities;
using Spectre.Console.Cli;
using System.ComponentModel;
namespace GD.Commands;
internal class UseCommand : Command<UseCommand.UseSettings>
{
    public class UseSettings : CommandSettings
    {
        [CommandArgument(0, "[version]")]
        [Description("Godot version to use. ")]
        public string Version { get; set; } = string.Empty;

        [CommandOption("--mono", isRequired: false)]
        [Description("Use the .NET (Mono) build of Godot.")]
        public bool UseMono { get; set; } = false;
        
        [CommandOption("--console", isRequired: false)]
        [Description("Run Godot via the console instead of launching the GUI.")]
        public bool UseConsole { get; set; } = false;

        [CommandOption("--global", isRequired:false)]
        [Description("Set the selected Godot version as the global default.")]
        public bool IsGlobal { get; set; }
        public bool HasVersion => Version != string.Empty;
    }

    public override int Execute(CommandContext context, UseSettings settings, CancellationToken cancellationToken)
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
        ////For testing purposes, I will just print the version for now.
        //ConsoleMarkupUtility.PrintInfo($"Using the version {settings.Version}");
        //ConsoleMarkupUtility.PrintInfo($"Is Global : {settings.IsGlobal}");
        //ConsoleMarkupUtility.PrintInfo($"Use Mono : {settings.UseMono}");
        //ConsoleMarkupUtility.PrintInfo($"Use console : {settings.UseConsole}");
        return 0;
    }
}
