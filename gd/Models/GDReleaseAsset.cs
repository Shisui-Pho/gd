namespace GD.Models;

public class GDReleaseAsset
{
    public string Url { get; set; }

    public int Id { get; set; }
    public string Name { get; set; }

    public string Label { get; set; }

    public string State { get; set; }

    public string ContentType { get; set; }

    public int Size { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public string BrowserDownloadUrl { get; set; }
}