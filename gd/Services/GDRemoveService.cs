using GD.Infrastructure.Abstractions;
using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
namespace GD.Services;
internal class GDRemoveService
{
    private readonly IGDConfigurations _configurations;
    private readonly IGodotVersionsManager _vManager;

    public GDRemoveService(IGodotVersionsManager vManager,IGDConfigurationsFactory factory)
    {
        _configurations = factory.ConfigurationsInstance;
        _vManager = vManager;
        _vManager.LoadVersions(_configurations);
    }
    public bool RemoveGodotVersion(string version, bool mono)
    {
        if(!_vManager.DataLoaded)
        {
            return false;
        }

        if(!GodotVersion.IsValidVersionFormat(version, out string standardVersion))
        {
            ConsoleMarkupUtility.PrintError("Invalid version format.");
            return false;
        }    
        GodotVersion gVersion = _vManager.GetByVersionString(standardVersion);
        if(gVersion == null)
        {
            ConsoleMarkupUtility.PrintError("The version is not currently installed on the machine.");
            return false;
        }

        //Here the version exists
        //-Remove version from configs
        _vManager.RemoveVersion(gVersion);

        //Delete the folder
        if(Directory.Exists(gVersion.Path))
        {
            Directory.Delete(gVersion.Path, true);
        }
        return true;
    }
}