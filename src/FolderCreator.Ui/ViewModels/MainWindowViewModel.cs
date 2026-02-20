using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FolderCreator.Ui.Services;

namespace FolderCreator.Ui.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly FolderCreatorService _service = new();

    [ObservableProperty]
    private string? baseDirectory;

    [ObservableProperty]
    private string? mainFolderName;

    public ObservableCollection<SubfolderItemViewModel> Subfolders { get; } = new();

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isSuccess;

    public MainWindowViewModel()
    {
        // start with one row so it feels "ready"
        Subfolders.Add(new SubfolderItemViewModel());
        UpdateCanCreate();
    }

    partial void OnBaseDirectoryChanged(string? value) => UpdateCanCreate();
    partial void OnMainFolderNameChanged(string? value) => UpdateCanCreate();

    private void UpdateCanCreate()
    {
        CreateCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void AddSubfolder()
    {
        Subfolders.Add(new SubfolderItemViewModel());
        UpdateCanCreate();
    }

    [RelayCommand]
    private void RemoveSubfolder(SubfolderItemViewModel item)
    {
        if (Subfolders.Contains(item))
            Subfolders.Remove(item);
        
        if (Subfolders.Count == 0)
            Subfolders.Add(new SubfolderItemViewModel());
        
        UpdateCanCreate();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        var result = _service.Create(
            baseDirectory: BaseDirectory ?? "",
            mainFolderName: (MainFolderName ?? "").Trim(),
            subfolderNames: Subfolders.Select(s => s.Name ?? "")
        );

        StatusMessage = result.Message;
        IsSuccess = result.Success;
    }

    private bool CanCreate()
        => !string.IsNullOrWhiteSpace(BaseDirectory)
            && !string.IsNullOrWhiteSpace(MainFolderName);
}
