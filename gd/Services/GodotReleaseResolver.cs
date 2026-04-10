using GD.Infrastructure;
using GD.Infrastructure.Abstractions;
using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
using GD.Utilities.Extensions;
using Octokit;
using System.Reflection;
using System.Text.Json;
namespace GD.Services;

internal class GodotReleaseResolver : IGodotReleaseResolver
{
    private const string ENV_PAT_NAME = "GITHUB_PAT";
    private const string GODOT_GITHUB_ACCOUNT_NAME = "godotengine";
    private const string GODOT_GITHUB_RELEASE_REPOSITORY = "godot-builds";


    private readonly string token = null!;
    private readonly IGDConfigurations configs;
    private string personalAccessToken;
    private string platform;
    public GodotReleaseResolver(IGDConfigurationsFactory factory)
    {
        configs = factory.ConfigurationsInstance;
        personalAccessToken = Environment.GetEnvironmentVariable(ENV_PAT_NAME) ?? string.Empty;
    }

    public async Task<GDReleaseAsset> GetReleaseAsset(GodotInstallParam installParam)
    {
        var gdRelease = await GetRelease(installParam.GenerateTagName());
        if (gdRelease == null) return null;

        //Filter down to the actual release asset needed by the user
        platform = configs.GDConf.GetPlatformFor(installParam.MonoBuild);
        //ConsoleMarkupUtility.PrintLine(JsonSerializer.Serialize(installParam));
        var asset = gdRelease.Assets.Where(x => x.Name.EndsWith(".zip")).FirstOrDefault(x => IsRequiredReleaseAssert(x, installParam));

        return asset;
    }
    private bool IsRequiredReleaseAssert(GDReleaseAsset assert, GodotInstallParam installParam)
    {
        if(configs.GDConf.OsType == OsType.Windows && !assert.Name.Contains("win"))
            return false;
        else if(configs.GDConf.OsType != OsType.Windows && !assert.Name.Contains(configs.GDConf.OsType.ToString(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (installParam.MonoBuild != assert.Name.Contains("mono", StringComparison.OrdinalIgnoreCase))
            return false;

        if (installParam.PlatformProvidedByUser && assert.Name.Contains(installParam.Platform))
            return true;

        if (!installParam.PlatformProvidedByUser && assert.Name.Contains(platform))
            return true;

        return false;//Not a match
    }
    public async Task<GDReleaseAsset> GetLatestRelease(bool includeMono)
    {
        return null;
    }
    public async Task<bool> UpdateCachedRelease()
    {
        _ = await GetAndCacheRelease();
        return true;
    }
    private async Task<GDRelease> GetRelease(string tagName)
    {
        List<GDRelease> releases;
        if (configs.HasCachedReleases)
        {
            //Read from the cached releases
            string contents = File.ReadAllText(configs.CachedReleaseFilePath);
            try
            {
                releases = JsonSerializer.Deserialize<List<GDRelease>>(contents);
            }
            catch
            {
                return null;
            }
        }
        else
        {
            releases = await GetAndCacheRelease();
        }

        if (releases.Count == 0) return null;

        var release = releases.FirstOrDefault(x => x.TagName == tagName);

        return release;
    }
    private async Task<List<GDRelease>> GetAndCacheRelease()
    {
        var header = new ProductHeaderValue(Globals.AppName, Assembly.GetEntryAssembly()?.GetName().Version.ToString());
        GitHubClient client = new(header);
        if (!string.IsNullOrEmpty(personalAccessToken))
        {
            client.Credentials = new Credentials(token);
        }

        //Create the file for the releases cache
        File.Create(configs.CachedReleaseFilePath).Close();

        var releases = await client.Repository.Release.GetAll(GODOT_GITHUB_ACCOUNT_NAME, GODOT_GITHUB_RELEASE_REPOSITORY);

        if (releases.Any())
        {
            //Cache the releases
            List<GDRelease> gdReleases = [.. releases.Select(x => x.ToGDRelease())];
            File.WriteAllText(configs.CachedReleaseFilePath, JsonSerializer.Serialize(gdReleases));
            return gdReleases;
        }
        return [];
    }
}