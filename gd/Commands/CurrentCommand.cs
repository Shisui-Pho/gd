using GD.Commands.Settings;
using GD.Infrastructure.Abstractions;
using GD.Services.Abstractions;
using GD.Utilities;
using Spectre.Console.Cli;

namespace GD.Commands;
internal class CurrentCommand : Command<DefaultSettings>
{
    private readonly IGDConfigurationsFactory _configFactory;
    private readonly IGodotVersionsManager _godotVersionManager;
    public CurrentCommand(IGDConfigurationsFactory configFactory, IGodotVersionsManager godotVersionManager)
    {
        _configFactory = configFactory;
        _godotVersionManager = godotVersionManager;
        _godotVersionManager.LoadVersions(_configFactory.ConfigurationsInstance);
    }
    public override int Execute(CommandContext context, DefaultSettings settings, CancellationToken cancellationToken)
    {
        if (context.Remaining.Parsed.Count > 0)
        {
            ConsoleMarkupUtility.PrintError($"Unexpected arguments passed");
            return 0;
        }
        
        if(!_godotVersionManager.DataLoaded)
        {
            return -1;//Stop the execution
        }

        if (_godotVersionManager.GlobalGodotVersion == null)
        {
            ConsoleMarkupUtility.PrintInfo("No global Godot version set.");
        }
        else
        {
            //Print the global version details:
            ConsoleMarkupUtility.PrintGodotVersion(_godotVersionManager.GlobalGodotVersion, "Global Godot Version");
        }
        return 0;
    }
}