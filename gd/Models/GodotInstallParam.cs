namespace GD.Models;

public class GodotInstallParam
{
    public string Version { get; set; }
    public string Channel { get; set; }
    public string Platform { get; set; } = string.Empty;
    public bool MonoBuild { get; set; }
    public bool PlatformProvidedByUser { get; set; } = false;
    public string GenerateTagName()
    {
        return $"{Version}-{Channel}";
    }
}
