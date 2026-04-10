using GD.Models;

namespace GD.Services.Abstractions
{
    internal interface IGDInstallService
    {
        void InstallGodot(string version, bool monoVersion, bool stable);
        Task InstallGodot(GodotInstallParam param, CancellationToken cancellationToken);
    }
}