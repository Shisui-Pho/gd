using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GD.Services;

internal class GDConfigurations : IGDConfigurations
{
    #region constants
    //Folders
    private const string BASE_FOLDER_NAME = "gd";
    private const string TEMP_DOWNLOAD_FOLDER_NAME = "temp";
    private const string VERSIONS_FOLDER_NAME = "versions";

    //Files 
    private const string RELATIVE_INSTALLED_VERSIONS_CONFIG_FILE_PATH = "config/versions.json";
    private const string RELATIVE_LOG_FILE_PATH = "logs/logs.txt";
    private const string RELATIVE_CONFIG_FILE_PATH = "config/config.json";
    private const string RELATIVE_CACHE_GODOT_RELEASE_FILE_PATH = "config/releases.json"; 


    //Environment variable for the path to the installation folder for the Godot binaries
    private const string ENV_VAR_INSTALLATION_FOLDER = "GD_HOME";

    #endregion constants


    #region fields
    private static readonly JsonSerializerOptions configOpt = new() { WriteIndented = true, PropertyNameCaseInsensitive = true, ReferenceHandler = ReferenceHandler.IgnoreCycles };
    private readonly string gdDataFolder;
    private readonly string gdLocalDataFolder;
    private bool hasCachedReleases = false;


    private GDConfiguration _config;
    #endregion fields



    public IGDConfiguration GDConf => _config;
    public string ConfigurationsFilePath { get; }
    public string LogFilePath { get; }
    public string VersionConfigurationsFilePath { get; }
    public string CachedReleaseFilePath { get; }
    public bool HasCachedReleases => hasCachedReleases;

    public GDConfigurations()
    {
        gdDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        //This determines the base path for the local download and version folder
        gdLocalDataFolder = GetGDHomeFolder(out var os);

        ConfigurationsFilePath = Path.Combine(gdDataFolder, BASE_FOLDER_NAME,RELATIVE_CONFIG_FILE_PATH);
        LogFilePath = Path.Combine(gdDataFolder, BASE_FOLDER_NAME,RELATIVE_LOG_FILE_PATH);
        VersionConfigurationsFilePath = Path.Combine(gdDataFolder, BASE_FOLDER_NAME, RELATIVE_INSTALLED_VERSIONS_CONFIG_FILE_PATH);
        CachedReleaseFilePath = Path.Combine(gdDataFolder, BASE_FOLDER_NAME, RELATIVE_CACHE_GODOT_RELEASE_FILE_PATH);

        //For configurations
        if (!TryLoadConfigurations())
        {
            string tempPath = Path.Combine(gdLocalDataFolder, TEMP_DOWNLOAD_FOLDER_NAME);
            string installPath = Path.Combine(gdLocalDataFolder, VERSIONS_FOLDER_NAME);
            _config = new()
            {
                InstallationsFolder = installPath,
                TempDownloadFolder = tempPath,
                IsLoaded = false,
            };
        }
        _config.OsType = os;
        EnsureStorageInitialized();
    }
    private void EnsureStorageInitialized()
    {
        //Root folders
        if (!string.IsNullOrEmpty(gdDataFolder) && !Directory.Exists(gdDataFolder))
        {
            Directory.CreateDirectory(gdDataFolder);
        }

        if (!string.IsNullOrEmpty(gdLocalDataFolder) && !Directory.Exists(gdLocalDataFolder))
        {
            Directory.CreateDirectory(gdLocalDataFolder);
        }

        //-For local data
        if (!string.IsNullOrEmpty(_config.InstallationsFolder) && !Directory.Exists(_config.InstallationsFolder))
        {
            Directory.CreateDirectory(_config.InstallationsFolder);
        }

        if (!string.IsNullOrEmpty(_config.TempDownloadFolder) && !Directory.Exists(_config.TempDownloadFolder))
        {
            Directory.CreateDirectory(_config.TempDownloadFolder);
        }

        //-For config and logs
        if (!string.IsNullOrEmpty(ConfigurationsFilePath))
        {
            var configDirectory = Path.GetDirectoryName(ConfigurationsFilePath);
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }
        }
        if (!string.IsNullOrEmpty(VersionConfigurationsFilePath))
        {
            var configDirectory = Path.GetDirectoryName(VersionConfigurationsFilePath);
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }
        }
        if (!string.IsNullOrEmpty(LogFilePath))
        {
            var logDirectory = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        //-Log files and config file
        if (!string.IsNullOrEmpty(ConfigurationsFilePath) && !File.Exists(ConfigurationsFilePath))
        {
            File.Create(ConfigurationsFilePath).Close();
            SaveConfigurations();
        }
        if (!string.IsNullOrEmpty(LogFilePath) && !File.Exists(LogFilePath))
        {
            File.Create(LogFilePath).Close();
        }
        if (!string.IsNullOrEmpty(VersionConfigurationsFilePath) && !File.Exists(VersionConfigurationsFilePath))
        {
            File.Create(VersionConfigurationsFilePath).Close();

            //-Make sure the file is in the correct format for JSON Deserialization later on
            var versions = GodotVersionsManager.GetEmptySerializationTemplate();
            File.WriteAllText(VersionConfigurationsFilePath,versions);
        }
        hasCachedReleases = File.Exists(CachedReleaseFilePath);
    }

    private bool TryLoadConfigurations()
    {
        if (!File.Exists(ConfigurationsFilePath))
        {
            ConsoleMarkupUtility.PrintWarning("GD config file missing, using default settings.");
            return false;
        }
        var jsonConf = File.ReadAllText(ConfigurationsFilePath);
        try
        {
            var config = JsonSerializer.Deserialize<GDConfiguration>(jsonConf);
            if (config != null)
            {
                _config = new()
                {
                    InstallationsFolder = config.InstallationsFolder,
                    TempDownloadFolder = config.TempDownloadFolder,
                };

                _config.IsLoaded = true;
            }
            return config != null;
        }
        catch(JsonException)
        {
            ConsoleMarkupUtility.PrintError($"A broken config file detected, using default configs.");
        }
        catch(Exception ex)
        {
            ConsoleMarkupUtility.HandleException(ex);
        }
        return false;
    }
    public void SaveConfigurations()
    {
        var json = JsonSerializer.Serialize(_config, configOpt);
        var configDirectory = Path.GetDirectoryName(ConfigurationsFilePath);
        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }
        File.WriteAllText(ConfigurationsFilePath, json);
    }
    private static string GetGDHomeFolder(out OsType osType)
    {
        osType = OperatingSystem.IsWindows() ? OsType.Windows :
                 OperatingSystem.IsLinux() ? OsType.Linux :
                 OperatingSystem.IsMacOS() ? OsType.MacOS : 
                 throw new PlatformNotSupportedException("Only Windows, Linux and MacOS is supported.");

        var basePath = Environment.GetEnvironmentVariable(ENV_VAR_INSTALLATION_FOLDER);
        if (!string.IsNullOrWhiteSpace(basePath))
            return basePath;

        basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(basePath, BASE_FOLDER_NAME);
    }
}