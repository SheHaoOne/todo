using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TodoApp.Converters;

public class ImportantStarConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isImportant = value is bool b && b;
        return isImportant
            ? new SolidColorBrush(Color.FromRgb(0xFF, 0xB9, 0x00))
            : new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xC8));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
