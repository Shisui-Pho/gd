using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace GD.Services;

internal class GodotVersionsManager : IGodotVersionsManager
{
    private class InternalVersionManager
    {
        public List<GodotVersion> GodotVersions { get; set; } = null;
        public GodotVersion GlobalGodotVersion { get; set; } = null;
    }
    private class GodotVersionComparer : IComparable<GodotVersion>
    {
        private readonly GodotVersion _version;
        private readonly int[] parts;
        public GodotVersionComparer(GodotVersion version)
        {
            _version = version;
            parts = GetParts(_version.Version);
        }
        private static int[] GetParts(string versionStr)
        {
            var parts = versionStr.Split('.').Select(int.Parse);
            return [.. parts];
        }
        public int CompareTo(GodotVersion other)
        {
            int[] otherParts = GetParts(other.Version);

            //Compare by parts
            for(int i = 0; i < parts.Length; i++)
            {
                int comparer = parts[i].CompareTo(otherParts[i]);
                if (comparer != 0)
                {
                    return comparer;
                }
            }

            //if they are the same
            //-Dotnet support comes after the standard build
            if (!_version.SupportsDotNet && other.SupportsDotNet)
                return -1;
            if (_version.SupportsDotNet && !other.SupportsDotNet)
                return 1;

            return 0;
        }
    }

    private InternalVersionManager internalVersionManager;
    private bool configChanged;
    private bool dataLoaded;
    public GodotVersion GlobalGodotVersion
    {
        get => internalVersionManager.GlobalGodotVersion;
        set
        {
            if (value is null)
            {
                internalVersionManager.GlobalGodotVersion = value;
                configChanged  = true;
                return;
            }

            //If not null, add extra validation

            if(value.IsBroken)
            {
                throw new InvalidOperationException("Cannot set a broken version as the global version.");
            }
            internalVersionManager.GlobalGodotVersion = value;
            configChanged = true;
        }
    }
    public IEnumerable<GodotVersion> GodotVersions
    {
        get
        {
            if (!dataLoaded)
                throw new InvalidOperationException("No version was loaded.");

            return internalVersionManager.GodotVersions;
        }
    }
    [JsonIgnore]
    public bool DataLoaded => dataLoaded;
    [JsonIgnore]
    public bool ConfigsChanged => configChanged;
    public GodotVersionsManager()
    {
        internalVersionManager = new();
        configChanged = false;
        dataLoaded = false;
    }
    public IEnumerable<GodotVersion> LoadVersions(IGDConfigurations configs)
    {
        if(dataLoaded) return internalVersionManager.GodotVersions;

        dataLoaded = true;
        configChanged = false;

        if (string.IsNullOrEmpty(configs.VersionConfigurationsFilePath))
        {
            internalVersionManager.GodotVersions ??= [];
            return internalVersionManager.GodotVersions;
        }

        if (!File.Exists(configs.VersionConfigurationsFilePath))
        {
            ConsoleMarkupUtility.PrintError("Version configurations file not found. Run `gd config repair` to repair configurations");
            return null;
        }

        var data = File.ReadAllText(configs.VersionConfigurationsFilePath);
        try
        {
            var versionsConf = JsonSerializer.Deserialize<InternalVersionManager>(data) ?? new();
            foreach (var item in versionsConf.GodotVersions)
            {
                AddVersion(item);
            }
            internalVersionManager.GodotVersions ??= [];//In case no versions were loaded from config files

            if(internalVersionManager.GlobalGodotVersion != null)
            {
                //Override version
                internalVersionManager.GlobalGodotVersion = versionsConf.GlobalGodotVersion;
            }
            return internalVersionManager.GodotVersions;
        }
        catch(JsonException)
        {
            ConsoleMarkupUtility.PrintError("A broken versions.conf file detected. Run `gd config repair --versions` to repair the file.");
            dataLoaded = false;
            return null; 
        }
        catch (Exception ex)
        {
            ConsoleMarkupUtility.HandleException(ex);
            dataLoaded = false;
            return null;
        }
    }
    public bool VersionExists(GodotVersion godotVer)
    {
        foreach (var item in internalVersionManager.GodotVersions)
        {
            if(item.IsSameVersion(godotVer))
                return true;
        }
        return false;
    }
    public void SaveChanges(IGDConfigurations configs)
    {
        if (!ConfigsChanged) return;

        try
        {
            string jsonData = JsonSerializer.Serialize(internalVersionManager);
            File.WriteAllText(configs.VersionConfigurationsFilePath, jsonData);
            configChanged = false;
        }
        catch (Exception ex)
        {
            ConsoleMarkupUtility.HandleException(ex);
        }
    }
    public void AddVersion(GodotVersion version)
    {
        internalVersionManager.GodotVersions ??= [];

        //Only add unique versions
        if (internalVersionManager.GodotVersions.Any(version.IsSameVersion))
            return;

        internalVersionManager.GodotVersions.Add(version);
        version.IsBroken = GodotVersion.IsGodotVersionBrocken(version);
        configChanged = true;
    }
    public GodotVersion GetByVersionString(string version, bool mono = false)
    {
        if(GodotVersion.IsValidVersionFormat(version, out string standardVersion))
        {
            var godotVersion = internalVersionManager.GodotVersions.FirstOrDefault(x => x.Version == standardVersion && x.SupportsDotNet == mono);
            return godotVersion;
        }
        return null;
    }
    public GodotVersion GetLatestVersion()
    {
        //Get the latest stable version, if there is no stable version, get the latest beta version, if there is no beta version, get the latest broken version
        var noneBrocken = internalVersionManager.GodotVersions.Where(v => !v.IsBroken);
 
        var v = noneBrocken.OrderByDescending(x => new GodotVersionComparer(x)).FirstOrDefault();

        return v;
    }
    public GodotVersion GetLatestVersion(string major, string minor = null)
    {
        var matchedVersions = internalVersionManager.GodotVersions.Where(v => !v.IsBroken && v.Version.StartsWith(major + "."));
        if(minor != null)
        {
            matchedVersions = matchedVersions?.Where(v =>v.Version.StartsWith(major + "." +  minor + "."));
        }

        if(matchedVersions == null || !matchedVersions.Any())
        {
            return null; 
        }

        matchedVersions = matchedVersions.OrderByDescending(v => new GodotVersionComparer(v));
        
        var v = matchedVersions.FirstOrDefault();
        return v;
    }
    public void RemoveVersion(GodotVersion version)
    {
        if (internalVersionManager.GodotVersions != null && internalVersionManager.GodotVersions.Contains(version))
        {
            internalVersionManager.GodotVersions.Remove(version);
            configChanged = true;
        }
    }
    public void RemoveVersion(string version)
    {
        if (internalVersionManager.GodotVersions != null)
        {
            var verToRemove = internalVersionManager.GodotVersions.FirstOrDefault(v => v.Version == version);
            if (verToRemove != null)
            {
                RemoveVersion(verToRemove);
            }
        }
    }
    public static string GetEmptySerializationTemplate()
    {
        return JsonSerializer.Serialize(new InternalVersionManager()
        {
            GlobalGodotVersion = null,
            GodotVersions = []
        });
    }
}