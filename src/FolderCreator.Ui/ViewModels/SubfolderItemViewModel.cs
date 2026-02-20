using CommunityToolkit.Mvvm.ComponentModel;

namespace FolderCreator.Ui.ViewModels;

public partial class SubfolderItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? name;
}
