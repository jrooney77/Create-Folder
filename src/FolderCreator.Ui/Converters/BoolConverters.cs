using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FolderCreator.Ui.Converters;

public static class BoolConverters
{
    public static readonly IValueConverter SuccessToBg =
        new FuncValueConverter<bool, IBrush>(success =>
            success
                ? new SolidColorBrush(Color.Parse("#2D6A4F"))
                : new SolidColorBrush(Color.Parse("#B04A4A")));
}