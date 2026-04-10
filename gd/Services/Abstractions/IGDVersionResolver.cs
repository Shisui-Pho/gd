namespace GD.Services.Abstractions
{
    internal interface IGDVersionResolver
    {
        string ResolveVersion(string version = null);
        string ResolveVersionExecutablePath(string version, bool useConsole = false, bool useMono = false);
        bool VersionExists(string version);
    }
}