using GD.Utilities;
using System.Diagnostics;

namespace GD.Services;

internal class GDGodotLauncher
{
    public static int LaunchGodot(string filePath)
    {
        if(!File.Exists(filePath))
            ConsoleMarkupUtility.PrintError($"The specified Godot executable was not found @{filePath}");

        var processInfo = new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        };
        try
        {
            Process.Start(processInfo);
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleMarkupUtility.PrintError($"Error launching Godot: {ex.Message}");
            //throw new InvalidOperationException($"Failed to launch Godot from '{filePath}'.", ex);
            return -1;
        }
    }

    public static int LaunchGodot(string exePath, string arguments = "", string workingDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(exePath))
            throw new ArgumentException("Executable path cannot be empty.", nameof(exePath));

        if (!File.Exists(exePath))
            throw new FileNotFoundException($"The specified Godot executable was not found at: {exePath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(exePath)!,
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleMarkupUtility.PrintError($"Error launching Godot: {ex.Message}");
            //throw new InvalidOperationException($"Failed to launch Godot from '{exePath}'.", ex);
            return -1;
        }
    }
}