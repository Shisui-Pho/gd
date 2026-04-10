using GD.Models;
using GD.Services.Abstractions;
using GD.Utilities;
using System;
namespace GD.Services;
internal class GDUseService
{
    private readonly IGDVersionResolver _resolver;
    private readonly IGodotVersionsManager _manager;

    public GDUseService(IGDVersionResolver resolver, IGodotVersionsManager manager)
    {
        _resolver = resolver;
        _manager = manager;
    }
    public (GodotVersion versionBefore,  GodotVersion versionAfter) UpdateGlobalVersion(string version, bool mono = false)
    {
        //First resolve the version
        string resolvedVersion = _resolver.ResolveVersion(version);
        if (resolvedVersion == null)
        {
            ConsoleMarkupUtility.PrintError($"Could not resolve the Godot version {version}.");
            return (null, null);
        }

        var versionBefore = _manager.GetByVersionString(resolvedVersion);
        return default;
    }
}
