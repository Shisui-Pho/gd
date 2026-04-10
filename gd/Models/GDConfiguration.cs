using GD.Services.Abstractions;
using GD.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GD.Models;
internal class GDConfiguration : IGDConfiguration
{
    //Property fields
    private OsType osType;
    private string platform;
    private string tempFolder;
    private string installationFolder;
    private bool isLoadedData;

    private bool configChanged;
    
    public string TempDownloadFolder
    {
        get => tempFolder;
        set
        {
            tempFolder = value;
            configChanged = true;
        }
    }
    public string InstallationsFolder
    {
        get => installationFolder;
        set
        {
            installationFolder = value;
            configChanged = true;
        }
    }
    [JsonIgnore]
    public bool IsLoaded
    {
        get => isLoadedData;
        set
        {
            if (value)
                configChanged = false; //Data was not changed
            isLoadedData = value;
        }
    }
    [JsonIgnore]
    public OsType OsType
    {
        get => osType;
        set
        {
            osType = value;
            platform = osType switch
            {
                OsType.Windows => "win64",
                OsType.Linux => "linux.x86_64",
                OsType.MacOS => "macos.universal",
                _ => string.Empty
            };
            IsLoaded = true;
        }
    }
    public string Platform  => platform;
    [JsonIgnore]
    public bool ConfigsChanged => configChanged;
    public GDConfiguration()
    {
        TempDownloadFolder = string.Empty;
        InstallationsFolder = string.Empty;
    }
    public string GetPlatformFor(bool monoBuild)
    {
        //If OS not linux, we can return as is
        if (OsType != OsType.Linux)
            return platform;

        if (!monoBuild) return Platform;

        //For mono builds, the platform string is a bit different than the standard build in Linux
        //- linux.x86_64   <--- Standard
        //- linux_x86_64   <--- mono build
        return Platform.Replace(".", "_");
    }
    public void SaveData(IGDConfigurations configs)
    {
        //This method is not exposed by the interface, as it is only be
        //  used internally by the service that manages the configurations,
        //  and it is not intended to be used by external classes.
        try
        {
            string jsonData = JsonSerializer.Serialize(this);
            File.WriteAllText(configs.VersionConfigurationsFilePath, jsonData);
            configChanged = false;
        }
        catch (Exception ex)
        {
            ConsoleMarkupUtility.HandleException(ex);
        }
    }
}