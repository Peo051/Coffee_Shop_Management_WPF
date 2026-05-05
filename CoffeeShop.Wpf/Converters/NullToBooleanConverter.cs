using System.Globalization;
using System.Windows.Data;

namespace CoffeeShop.Wpf.Converters;

/// <summary>
/// Converter: null → false, not null → true
/// </summary>
public sealed class NullToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
