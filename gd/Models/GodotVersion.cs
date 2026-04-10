namespace GD.Models;
internal class GodotVersion
{
    private static readonly string[] versionSuffix = ["stable", "beta", "alpha", "rc", "preview", "dev", "nightly", "lts", "sts"];
    private string _version = string.Empty;
    /// <summary>
    /// Gets or sets the version identifier for the current instance. 
    /// </summary>
    public string Version
    {
        get => _version;
        set
        {           
            if(!IsValidVersionFormat(value, out string version))
                throw new ArgumentException("Version must be in a valid format (e.g., '3.5.1', '4.0.0-beta').", nameof(value));

            //Standardize version string

            _version = version;
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the Godot version is currently active. 
    /// </summary>
    public bool IsActive { get; set; } = false;
    /// <summary>
    /// Gets or sets a value indicating whether the object is in a broken state.
    /// </summary>
    public bool IsBroken { get; set; } = false;
    /// <summary>
    /// Gets or sets the folder path where the Godot executable(s) for this version is/are located. 
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether .NET is supported by the current environment.
    /// In older versions of Godot, .NET support was provided through Mono, while in newer versions, .NET support is built-in.
    /// </summary>
    public bool SupportsDotNet { get; set; } = false;
    /// <summary>
    /// Gets or sets a value indicating the Godot version channel(e.g, Stable, Beta, Dev, etc)
    /// </summary>
    public string Channel {  get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the file system path to the console executable used by the application.
    /// </summary>
    public string ConsoleExecutablePath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the file system path to the GUI executable.
    /// </summary>
    public string GuiExecutablePath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the current version supports stateful operations.
    /// </summary>
    public bool IsStatableVersion { get; set; } = false;
    public bool IsSameVersionString(string version)
    {
        if (string.IsNullOrEmpty(version))
            return false;

        //Standardize the version string by removing any leading 'v' and trimming whitespace
        if(version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            version = version[1..];

        string thisVersion = this.Version.Trim();

        if (thisVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            thisVersion = thisVersion[1..];

        return thisVersion == version;
    }
    public bool IsSameVersion(GodotVersion other)
    {
        if (other == null)
            return false;
        return IsSameVersionString(other.Version) && 
               SupportsDotNet == other.SupportsDotNet &&
               Channel == other.Channel;
    }
    public override string ToString()
    {
        return $"{Version} (Active: {IsActive}, Stable: {IsStatableVersion}, Broken: {IsBroken}, Supports .NET: {SupportsDotNet})";
    }

    #region Public Static Version Methods
    public static string GenerateExecutableFileName(string version, OsType osType, bool isMono = false, bool isConsole = false)
    {
        string consoleSuffix = isConsole ? "console" : "gui";
        string extension = osType == OsType.Windows ? ".exe" : 
                           osType == OsType.MacOS ? ".app" :
                           "";
        var folderName = GenerateVersionFolderName(version, isMono);

        return $"Godot_{folderName}_{consoleSuffix}{extension}";
    }
    public static string GenerateVersionFolderName(string version,bool isMono = false)
    {
        string monoSuffix = isMono ? "mono" : "standard";
        if(version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            version = version[1..];
        return $"v{version}_{monoSuffix}";
    }
    public static bool IsGodotVersionBrocken(GodotVersion version)
    {
        //Check for the presence of the console executable as an indicator of a broken version
        if (string.IsNullOrEmpty(version.ConsoleExecutablePath) || !File.Exists(version.ConsoleExecutablePath))
            return true;

        //Check for the GUI as well 
        if(string.IsNullOrEmpty(version.GuiExecutablePath) || !File.Exists(version.GuiExecutablePath))
            return true;

        return false;
    }
    public static bool IsValidVersionFormat(string versionToValidate, out string standardVersion)
    {
        standardVersion = versionToValidate?.Trim();

        if (string.IsNullOrEmpty(standardVersion))
            return false;

        standardVersion = standardVersion.Trim();

        //If leading 'v' exists, remove it for validation
        if (standardVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            standardVersion = standardVersion[1..];

        if (string.IsNullOrEmpty(standardVersion))
            return false;

        foreach (var suffix in versionSuffix)
        {
            if (standardVersion.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                standardVersion = standardVersion[..^suffix.Length].TrimEnd('-');
                break;
            }
        }

        standardVersion = standardVersion.Trim();

        if (string.IsNullOrEmpty(standardVersion))
            return false;

        standardVersion = ToStandardVersion(standardVersion);

        return !string.IsNullOrEmpty(standardVersion);
    }
    private static string ToStandardVersion(string version)
    {

        if (!version.Any(x => x == '.') && int.TryParse(version, out _))
        {
            return $"{version}.0.0";
        }

        var parts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            if (parts.Length == 2 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
                return $"{version}.0";
            if (parts.Length == 3 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _) && int.TryParse(parts[2], out _))
                return version;
        }
        return null;
    }
    #endregion Public Static Version Methods
}