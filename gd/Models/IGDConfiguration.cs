using System.Text.Json.Serialization;

namespace GD.Models;

internal interface IGDConfiguration
{
    string TempDownloadFolder { get; }
    string InstallationsFolder { get; }
    string Platform { get; }

    [JsonIgnore]
    bool IsLoaded { get; }
    [JsonIgnore]
    OsType OsType { get; }
    [JsonIgnore]
    bool ConfigsChanged { get; }

    string GetPlatformFor(bool mono);
}
