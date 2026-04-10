namespace GD.Models;

internal class GDRelease
{
    public string Url { get; set; }

    public string UploadUrl { get; set; }

    public long Id { get; set; }

    public string TagName { get; set; }

    public string Name { get; set; }

    public string Body { get; set; }

    public bool Draft { get; set; }

    public bool Prerelease { get; set; }
    public List<GDReleaseAsset> Assets { get; set; }
}
