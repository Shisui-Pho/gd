using GD.Models;
using SharpCompress.Archives;
using SharpCompress.Common;
using Spectre.Console;

namespace GD.Utilities;

internal static class FolderManager
{
    public static async Task<ServiceResult> ExtractZip(string zipPath, string destination, CancellationToken cancellationToken, delReportProgress progress = null)
    {
        try
        {
            using var archive = ArchiveFactory.OpenArchive(zipPath);
            long totalSize = archive.Entries.Sum(x => x.Size);
            long extracted = 0;
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    await entry.WriteToDirectoryAsync(destination, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                    }, cancellationToken);

                    extracted += entry.Size;
                    progress?.Invoke(extracted, totalSize);
                }
            }

            //Once the extraction is done, reposition the folders and files
            var success = RemoveRootExtractFolder(destination);

            if (success) return ServiceResult.Success();

            return ServiceResult.Fail("Unable to complete extraction operation");
        }
        catch (OperationCanceledException) 
        {
            return ServiceResult.Fail("Operation was cancelled.");
        }
        catch(Exception ex)
        {
            return ServiceResult.Fail(ex);
        }
    }
    public static bool MakeDirectory(string destPath)
    {
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
            return true;
        }

        var dir = new DirectoryInfo(destPath);

        if (dir.EnumerateDirectories().Any() || dir.EnumerateFiles().Any())
        {
            ConsoleMarkupUtility.PrintWarning($"The destination directory `{destPath}` is not empty, all content of the directory will be deleted.");

            var confirmed = AnsiConsole.Confirm("Continue?", false);

            if (confirmed)
            {
                try
                {
                    dir.Delete(true);
                    dir.Create();
                }
                catch (Exception ex)
                {
                    ConsoleMarkupUtility.HandleException(ex);
                    return false;
                }
            }
            return confirmed;
        }
        return true;
    }
    public static bool RemoveRootExtractFolder(string destPath)
    {
        var baseDir = new DirectoryInfo(destPath);

        if (!baseDir.Exists) return false;

        //When extracting from Godot, there will be one folder in root of "destPath"
        //-The folder will start with the name "Godot_"
        //-- we will move everything inside that folder 1 folder up and delete the folder.

        var files = baseDir.GetFiles();
        if(files.Length > 0) return true;

        var godotSub = baseDir.GetDirectories().FirstOrDefault(x => x.Name.StartsWith("Godot", StringComparison.OrdinalIgnoreCase));

        if (godotSub != null)
        {
            var subFiles = godotSub.GetFiles();
            var subFolders = godotSub.GetDirectories();

            foreach (var file in subFiles)
            {
                file.MoveTo(Path.Combine(destPath, file.Name));
            }
            foreach(var dir in subFolders)
            {
                dir.MoveTo(Path.Combine(destPath,dir.Name));
            }
            godotSub.Delete();//There shouldn't be anything left there, so this should work fine without recursive delete
        }
        return true;
    }
}