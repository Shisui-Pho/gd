namespace GD.Utilities;

//[Obsolete(message: "Use something else, XD")]
internal static class VersionsUtility
{
    [Obsolete(message: "Use something else, XD")]
    public static List<string> GetFolderNamesAsVersionNumbers(string path)
    {
        if (!Directory.Exists(path))
        {
            return new List<string>();
        }
        //Folder name structure: "vX.Y.Z_mono" or "vX.Y.Z_standard"
        // We will also order by descending order to prioritize latest "standard" version over "mono" if both exist for the same version number,
        // since "standard" is more commonly used and likely to be the default choice for users, while "mono" is often used for
        // specific use cases that may not be relevant to all users.

        return Directory.GetDirectories(path)
                        .Select(Path.GetFileName)
                        .OrderByDescending(x => x) 
                        .Select(folderName =>
                        {
                            //Extract the version part from the folder name
                            var parts = folderName.Split('_');
                            if (parts.Length > 0)
                            {
                                var versionPart = parts[0];
                                if (versionPart.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                                {
                                    versionPart = versionPart[1..];
                                }
                                return versionPart;
                            }
                            return null;
                        })
                        .Where(v => !string.IsNullOrEmpty(v)) //Filter out any null or empty version strings
                        .ToList();
    }
    public static string GetLatestVersion(string path)
    {
        var versions = GetFolderNamesAsVersionNumbers(path);
        if (versions.Count == 0)
            return null;
        var latestVersion = versions.OrderByDescending(v => new Version(v)).FirstOrDefault();
        return latestVersion;
    }
    public static bool TryGetVersionVersionFolder(string path, string version, bool isMono, out string folder)
    {
        if (!Directory.Exists(path))
        {
            folder = null;
            return false;
        }
        string expectedFolderName = $"v{version}_{(isMono ? "mono" : "standard")}";
        if(Directory.GetDirectories(path)
                        .Select(Path.GetFileName)
                        .Any(folderName => string.Equals(folderName, expectedFolderName, StringComparison.OrdinalIgnoreCase)))
        {
            folder = expectedFolderName;
            return true;
        }
        folder = null;
        return false;
    }
}
