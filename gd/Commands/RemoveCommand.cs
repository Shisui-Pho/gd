using GD.Utilities;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
namespace GD.Commands;
internal class RemoveCommand : Command<RemoveCommand.RemoveSettings>
{
    public class RemoveSettings : CommandSettings
    {
        [CommandArgument(0, "[version]")]
        [Description("Godot version to remove. ")]
        public string Version { get; set; } = string.Empty;
        
        [CommandOption("--mono", isRequired: false)]
        [Description("Remove the .NET (Mono) build of Godot.")]
        [DefaultValue(false)]
        public bool UseMono { get; set; } = false;
        
        public bool HasVersion => Version != string.Empty;
    }
    public override int Execute(CommandContext context, RemoveSettings settings, CancellationToken cancellationToken)
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
        //For testing purposes, I will just print the version for now.
        ConsoleMarkupUtility.PrintInfo($"Removing the version {settings.Version}");
        ConsoleMarkupUtility.PrintInfo($"Use Mono : {settings.UseMono}");
        return 0;
    }
}
