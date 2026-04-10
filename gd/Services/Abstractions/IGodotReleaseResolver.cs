using GD.Models;

namespace GD.Services.Abstractions
{
    internal interface IGodotReleaseResolver
    {
        Task<GDReleaseAsset> GetLatestRelease(bool includeMono);
        Task<GDReleaseAsset> GetReleaseAsset(GodotInstallParam installParam);
        Task<bool> UpdateCachedRelease();
    }
}