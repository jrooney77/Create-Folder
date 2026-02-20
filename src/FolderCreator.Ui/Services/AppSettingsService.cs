using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FolderCreator.Ui.Models;

namespace FolderCreator.Ui.Services;

public sealed class AppSettingsService
{
    private const string AppFolderName = "FolderCreator";
    private const string FileName = "settings.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsFilePath;

    public AppSettingsService()
    {
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsFilePath = Path.Combine(appDataDirectory, AppFolderName, FileName);
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsFilePath))
            return new AppSettings();

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (IOException)
        {
            return new AppSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var directoryPath = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        await File.WriteAllTextAsync(_settingsFilePath, json, cancellationToken);
    }
}
