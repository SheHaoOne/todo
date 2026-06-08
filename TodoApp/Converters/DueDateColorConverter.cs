using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TodoApp.ViewModels;

namespace TodoApp.Converters;

public class DueDateColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskItemViewModel task)
            return new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));

        if (task.IsOverdue)
            return new SolidColorBrush(Color.FromRgb(0xD1, 0x34, 0x38));

        if (task.IsDueToday)
            return new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));

        return new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
