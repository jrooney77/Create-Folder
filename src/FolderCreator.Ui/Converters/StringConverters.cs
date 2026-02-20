using System;
using Avalonia.Data.Converters;

namespace FolderCreator.Ui.Converters;

public static class StringConverters
{
    public static readonly IValueConverter NotNullOrEmpty =
        new FuncValueConverter<string?, bool>(s => !string.IsNullOrWhiteSpace(s));
}