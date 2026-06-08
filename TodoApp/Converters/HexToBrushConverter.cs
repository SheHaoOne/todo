using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TodoApp.Converters;

public class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            return new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex)!;
            return new SolidColorBrush(color);
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
