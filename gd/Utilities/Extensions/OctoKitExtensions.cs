using GD.Models;
using Octokit;

namespace GD.Utilities.Extensions;

internal static class OctoKitExtensions
{
    public static GDRelease ToGDRelease(this Release release)
    {
        return new()
        {
            Name = release.Name,
            UploadUrl = release.UploadUrl,
            Body = release.Body,
            Draft = release.Draft,
            Id = release.Id,
            Prerelease = release.Prerelease,
            TagName = release.TagName,
            Url = release.Url,
            Assets = release.Assets.Select(x => x.ToGDReleaseAsset()).ToList(),
        };
    }
    public static GDReleaseAsset ToGDReleaseAsset(this ReleaseAsset asset)
    {
        return new()
        {
            Url = asset.Url,
            BrowserDownloadUrl = asset.BrowserDownloadUrl,
            ContentType = asset.ContentType,
            CreatedAt = asset.CreatedAt,
            Id = asset.Id,
            Label = asset.Label,
            Name = asset.Name,
            Size = asset.Size,
            State = asset.State,
            UpdatedAt = asset.UpdatedAt,
        };
    }
}