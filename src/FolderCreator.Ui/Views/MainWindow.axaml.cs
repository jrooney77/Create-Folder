using Avalonia.Controls;
using Avalonia.Interactivity;
using FolderCreator.Ui.ViewModels;

namespace FolderCreator.Ui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseBaseDirectory_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var picker = await topLevel.StorageProvider.OpenFolderPickerAsync(new()
        {
            Title = "Select Base Directory",
            AllowMultiple = false
        });

        var folder = picker.Count > 0 ? picker[0] : null;
        if (folder is null) return;

        if (DataContext is MainWindowViewModel vm)
        {
            vm.BaseDirectory = folder.Path.LocalPath;
        }
    }
}