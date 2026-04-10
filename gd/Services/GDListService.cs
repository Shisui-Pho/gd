using GD.Infrastructure.Abstractions;
using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
namespace GD.Services;

internal class GDListService : IGDListService
{
    private readonly IGDConfigurationsFactory _configs;
    private readonly IGodotVersionsManager _versionsManager;
    public GDListService(IGDConfigurationsFactory configs, IGodotVersionsManager versionsManager)
    {
        _configs = configs;
        _versionsManager = versionsManager;
        _versionsManager.LoadVersions(_configs.ConfigurationsInstance);
    }
    public void ListAllGodotVersions()
    {
        if(_versionsManager.DataLoaded)
        {
            var versions = _versionsManager.GodotVersions;
            ConsoleMarkupUtility.PrintGodotVersionsTable(versions);
        }
    }
    public void ListMonoBuildGodotVersions()
    {
        if(_versionsManager.DataLoaded)
        {
            var versions = _versionsManager.GodotVersions;
            ConsoleMarkupUtility.PrintGodotVersionsTable(versions.Where(v => v.SupportsDotNet).ToList());
        }
    }
}