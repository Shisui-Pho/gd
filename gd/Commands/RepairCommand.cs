using GD.Infrastructure.Abstractions;
using Spectre.Console.Cli;

namespace GD.Commands;
internal class RepairCommand : Command<RepairCommand.RepairSettings>
{
    private readonly IGDConfigurationsFactory _gdFactory;
    public RepairCommand(IGDConfigurationsFactory gdFactory)
    {
        _gdFactory = gdFactory;
    }
    public override int Execute(CommandContext context, RepairSettings settings, CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
        return -1;
    }

    public class RepairSettings : CommandSettings
    {
        //public bool 
    }
}

internal class GDRepairService
{
    //public bool RepairGDConfigFiles()
    //{

    //}
}