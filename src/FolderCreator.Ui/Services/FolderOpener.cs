using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace FolderCreator.Ui.Services;

public sealed class FolderOpener
{
    public Result Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Result.Fail("No folder selected to open.");

        if (!Directory.Exists(path))
            return Result.Fail("The folder no longer exists.");

        try
        {
            var startInfo = CreateStartInfo(path);
            Process.Start(startInfo);
            return Result.Ok($"Opened folder: {path}");
        }
        catch (Win32Exception)
        {
            return Result.Fail("Could not open the folder with the system file manager.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to open folder: {ex.Message}");
        }
    }

    private static ProcessStartInfo CreateStartInfo(string path)
    {
        if (OperatingSystem.IsMacOS())
            return BuildStartInfo("open", path);

        if (OperatingSystem.IsWindows())
            return BuildStartInfo("explorer", path);

        return BuildStartInfo("xdg-open", path);
    }

    private static ProcessStartInfo BuildStartInfo(string fileName, string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(path);
        return startInfo;
    }
}
