using CommunityToolkit.Mvvm.ComponentModel;
using FolderCreator.Ui.Services;

namespace FolderCreator.Ui.ViewModels;

public partial class SubfolderItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? name;

    partial void OnNameChanged(string? value)
    {
        var sanitizedName = NameSanitizer.SanitizeFolderName(value);
        if (value != sanitizedName)
            Name = sanitizedName;
    }
}
