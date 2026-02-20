using System.IO;

namespace FolderCreator.Ui.Services;

public static class NameSanitizer
{
    public static string SanitizeFolderName(string? name)
    {
        var trimmedName = (name ?? string.Empty).Trim();
        if (trimmedName.Length == 0)
            return string.Empty;

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitizedName = trimmedName;

        foreach (var invalidCharacter in invalidCharacters)
            sanitizedName = sanitizedName.Replace(invalidCharacter.ToString(), string.Empty);

        return sanitizedName;
    }
}
