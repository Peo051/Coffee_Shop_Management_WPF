using System;
using System.Globalization;
using System.Windows.Data;

namespace CoffeeShop.Wpf.Converters;

/// <summary>
/// Chuyển đổi bool IsExpanded thành icon mũi tên
/// true (expanded) -> ▾
/// false (collapsed) -> ▸
/// </summary>
public sealed class BoolToExpandIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "▾" : "▸";
        }
        return "▸";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
