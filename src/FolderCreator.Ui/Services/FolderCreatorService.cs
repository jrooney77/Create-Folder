using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;

namespace FolderCreator.Ui.Services;

public sealed class FolderCreatorService
{
    public Result Create(string baseDirectory, string mainFolderName, IEnumerable<string> subfolderNames)
    {
        if (string.IsNullOrEmpty(baseDirectory))
            return Result.Fail("Please select a base directory.");

        if (!Directory.Exists(baseDirectory))
            return Result.Fail("The selected base directory does not exist.");

        if (string.IsNullOrWhiteSpace(mainFolderName))
            return Result.Fail("Please enter a name for the main folder.");

        // Clean + validate subfolder names
        var subs = subfolderNames
            .Select(s => (s ?? "").Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        var invalid = new List<string>();

        void CheckInvalid(string name)
        {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                invalid.Add(name);
        }

        CheckInvalid(mainFolderName);
        foreach (var s in subs) CheckInvalid(s);

        if (invalid.Count > 0)
            return Result.Fail($"The following folder names are invalid: {string.Join(", ", invalid)}");
        
        try
        {
            var rootPath = Path.Combine(baseDirectory, mainFolderName);
            // create main folder if missing
            Directory.CreateDirectory(rootPath);

            foreach (var sub in subs)
            {
                Directory.CreateDirectory(Path.Combine(rootPath, sub));
            }

            return Result.Ok($"Created {1 + subs.Count} folder(s) at: {rootPath}.");
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Fail("You do not have permission to create folders in the selected base directory.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while creating folders: {ex.Message}");
        }
    }
}

public readonly record struct Result(bool Success, string Message)
{
    public static Result Ok(string message) => new(true, message);
    public static Result Fail(string message) => new(false, message);
}