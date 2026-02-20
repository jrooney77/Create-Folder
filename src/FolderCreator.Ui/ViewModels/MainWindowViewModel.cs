using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FolderCreator.Ui.Models;
using FolderCreator.Ui.Services;

namespace FolderCreator.Ui.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly FolderCreatorService _service = new();
    private readonly FolderOpener _folderOpener = new();
    private readonly AppSettingsService _appSettingsService = new();

    public Func<Task<string?>>? PickFolderAsync { get; set; }

    [ObservableProperty]
    private string? baseDirectory;

    [ObservableProperty]
    private string? mainFolderName;

    public ObservableCollection<SubfolderItemViewModel> Subfolders { get; } = new();

    [ObservableProperty]
    private string? previewRootPath;

    public ObservableCollection<string> PreviewItems { get; } = new();

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isSuccess;

    [ObservableProperty]
    private string? lastCreatedPath;

    public MainWindowViewModel()
    {
        // start with one row so it feels "ready"
        AddSubfolderItem(new SubfolderItemViewModel());
        UpdateCanCreate();
        UpdatePreview();
    }

    partial void OnBaseDirectoryChanged(string? value)
    {
        UpdateCanCreate();
        UpdatePreview();

        var trimmedValue = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedValue))
            return;

        _ = SaveLastBaseDirectoryAsync(trimmedValue);
    }

    partial void OnMainFolderNameChanged(string? value)
    {
        var sanitizedName = NameSanitizer.SanitizeFolderName(value);
        if (value != sanitizedName)
        {
            MainFolderName = sanitizedName;
            return;
        }

        UpdateCanCreate();
        UpdatePreview();
    }

    partial void OnLastCreatedPathChanged(string? value)
    {
        OpenLastCreatedCommand.NotifyCanExecuteChanged();
    }

    private void UpdateCanCreate()
    {
        CreateCommand.NotifyCanExecuteChanged();
    }

    public async Task InitializeAsync()
    {
        var settings = await _appSettingsService.LoadAsync();
        var lastBaseDirectory = (settings.LastBaseDirectory ?? string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(lastBaseDirectory) && Directory.Exists(lastBaseDirectory))
            BaseDirectory = lastBaseDirectory;
    }

    [RelayCommand]
    private void AddSubfolder()
    {
        AddSubfolderItem(new SubfolderItemViewModel());
        UpdateCanCreate();
    }

    [RelayCommand]
    private void RemoveSubfolder(SubfolderItemViewModel item)
    {
        if (Subfolders.Contains(item))
            RemoveSubfolderItem(item);

        if (Subfolders.Count == 0)
            AddSubfolderItem(new SubfolderItemViewModel());

        UpdateCanCreate();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        var baseDirectory = (BaseDirectory ?? string.Empty).Trim();
        var mainFolderName = (MainFolderName ?? string.Empty).Trim();
        var rootPath = Path.Combine(baseDirectory, mainFolderName);

        var result = _service.Create(
            baseDirectory: baseDirectory,
            mainFolderName: mainFolderName,
            subfolderNames: Subfolders.Select(s => s.Name ?? "")
        );

        StatusMessage = result.Message;
        IsSuccess = result.Success;

        if (result.Success)
            LastCreatedPath = rootPath;
    }

    [RelayCommand(CanExecute = nameof(CanOpenLastCreated))]
    private void OpenLastCreated()
    {
        if (string.IsNullOrWhiteSpace(LastCreatedPath))
            return;

        var result = _folderOpener.Open(LastCreatedPath);
        StatusMessage = result.Message;
        IsSuccess = result.Success;
    }

    [RelayCommand]
    private async Task BrowseBaseDirectoryAsync()
    {
        if (PickFolderAsync is null)
            return;

        var pickedDirectory = await PickFolderAsync();
        if (!string.IsNullOrWhiteSpace(pickedDirectory))
            BaseDirectory = pickedDirectory;
    }

    private bool CanCreate()
        => !string.IsNullOrWhiteSpace(BaseDirectory)
            && !string.IsNullOrWhiteSpace(MainFolderName);

    private bool CanOpenLastCreated()
        => !string.IsNullOrWhiteSpace(LastCreatedPath)
            && Directory.Exists(LastCreatedPath);

    private void AddSubfolderItem(SubfolderItemViewModel item)
    {
        item.PropertyChanged += OnSubfolderPropertyChanged;
        Subfolders.Add(item);
        UpdatePreview();
    }

    private void RemoveSubfolderItem(SubfolderItemViewModel item)
    {
        item.PropertyChanged -= OnSubfolderPropertyChanged;
        Subfolders.Remove(item);
        UpdatePreview();
    }

    private void OnSubfolderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SubfolderItemViewModel.Name))
            UpdatePreview();
    }

    private void UpdatePreview()
    {
        var basePath = (BaseDirectory ?? string.Empty).Trim();
        var mainName = (MainFolderName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(mainName))
        {
            PreviewRootPath = null;
            PreviewItems.Clear();
            return;
        }

        var rootPath = Path.Combine(basePath, mainName);
        PreviewRootPath = rootPath;

        PreviewItems.Clear();
        PreviewItems.Add(rootPath);

        foreach (var subfolderName in Subfolders
                     .Select(s => (s.Name ?? string.Empty).Trim())
                     .Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            PreviewItems.Add(Path.Combine(rootPath, subfolderName));
        }
    }

    private async Task SaveLastBaseDirectoryAsync(string baseDirectoryPath)
    {
        try
        {
            await _appSettingsService.SaveAsync(new AppSettings
            {
                LastBaseDirectory = baseDirectoryPath
            });
        }
        catch (IOException)
        {
            // Ignore settings save errors to avoid interrupting the core workflow.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore settings save errors to avoid interrupting the core workflow.
        }
    }
}
