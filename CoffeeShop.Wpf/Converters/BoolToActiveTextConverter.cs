using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoffeeShop.Wpf.Converters;

/// <summary>
/// Chuyển đổi bool IsActive thành màu text
/// true -> trắng sáng
/// false -> màu xám nhạt
/// </summary>
public sealed class BoolToActiveTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
        {
            return Brushes.White;
        }
        // Màu xám nhạt cho item không active
        return new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)); // #C8FFFFFF
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
