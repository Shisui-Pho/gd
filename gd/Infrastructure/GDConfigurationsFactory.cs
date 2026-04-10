using GD.Infrastructure.Abstractions;
using GD.Services;
using GD.Services.Abstractions;
namespace GD.Infrastructure;

internal class GDConfigurationsFactory : IGDConfigurationsFactory
{
    private IGDConfigurations _configs;

    public IGDConfigurations ConfigurationsInstance
    {
        get => CreateConfigurations();
    }
    public IGDConfigurations CreateConfigurations()
    {
        _configs ??= new GDConfigurations();
        return _configs;
    }

    public bool GDConfigurationsLoaded()
    {
        return _configs != null;
    }
}