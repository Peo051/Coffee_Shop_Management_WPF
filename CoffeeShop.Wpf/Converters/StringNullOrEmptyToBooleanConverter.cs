using System.Globalization;
using System.Windows.Data;

namespace CoffeeShop.Wpf.Converters;

/// <summary>
/// Converter: string null/empty → true, có giá trị → false
/// </summary>
public sealed class StringNullOrEmptyToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
