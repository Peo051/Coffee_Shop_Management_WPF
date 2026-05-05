using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoffeeShop.Wpf.Converters;

/// <summary>
/// Chuyển đổi bool IsActive thành màu nền
/// true -> màu nâu sáng (active)
/// false -> transparent
/// </summary>
public sealed class BoolToActiveBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
        {
            // Màu nâu sáng cho active item
            return new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)); // #28FFFFFF
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
