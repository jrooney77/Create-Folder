using System.Threading.Tasks;
using Avalonia.Controls;
using FolderCreator.Ui.ViewModels;

namespace FolderCreator.Ui.Views;

public partial class MainWindow : Window
{
    private bool _hasInitializedViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => TryInitializeViewModel();
    }

    private async Task<string?> PickBaseDirectoryAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return null;

        var picker = await topLevel.StorageProvider.OpenFolderPickerAsync(new()
        {
            Title = "Select Base Directory",
            AllowMultiple = false
        });

        var folder = picker.Count > 0 ? picker[0] : null;
        return folder?.Path.LocalPath;
    }

    private void TryInitializeViewModel()
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.PickFolderAsync = PickBaseDirectoryAsync;

        if (_hasInitializedViewModel)
            return;

        _hasInitializedViewModel = true;
        _ = vm.InitializeAsync();
    }
}
