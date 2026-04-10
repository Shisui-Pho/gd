using GD.Models;
using System.Text.Json.Serialization;

namespace GD.Services.Abstractions;
internal interface IGodotVersionsManager
{
    [JsonIgnore]
    bool ConfigsChanged { get; }
    GodotVersion GlobalGodotVersion { get; set; }
    IEnumerable<GodotVersion> GodotVersions { get; }
    bool DataLoaded { get; }

    void AddVersion(GodotVersion version);
    GodotVersion GetByVersionString(string version, bool mono = false);
    GodotVersion GetLatestVersion();
    GodotVersion GetLatestVersion(string major, string minor = null);
    IEnumerable<GodotVersion> LoadVersions(IGDConfigurations configs);
    void RemoveVersion(GodotVersion version);
    void RemoveVersion(string version);
    void SaveChanges(IGDConfigurations configs);
    bool VersionExists(GodotVersion godotVer);
}