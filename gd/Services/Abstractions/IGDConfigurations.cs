using GD.Models;

namespace GD.Services.Abstractions
{
    internal interface IGDConfigurations
    {
        IGDConfiguration GDConf { get; }
        string ConfigurationsFilePath { get; }
        string VersionConfigurationsFilePath { get; }
        string CachedReleaseFilePath { get; }
        bool HasCachedReleases { get; }

        void SaveConfigurations();
    }
}