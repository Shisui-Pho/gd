using GD.Services.Abstractions;
using Spectre.Console.Cli;
namespace GD.Commands;

internal class ListCommand : Command<ListCommand.ListSettings>
{
    private readonly IGDListService _listService;
    public class ListSettings : CommandSettings
    {
        [CommandOption("--mono", isRequired: false)]
        public bool OnlyMono { get; set; } = false;
    }

    public ListCommand(IGDListService listService)
    {
        _listService = listService;
    }
    public override int Execute(CommandContext context, ListSettings settings, CancellationToken token)
    {
        //The list service will be removed and the configurations will have the versions
        //-But for now, we can just load the versions from the list service and print them
        if(settings.OnlyMono)
            _listService.ListMonoBuildGodotVersions();
        else
            _listService.ListAllGodotVersions();
        return 0;
    }
}