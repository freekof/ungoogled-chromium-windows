using System.Globalization;
using System.Windows.Data;

namespace FpBrowserLauncher.Converters;

/// <summary>
/// 将字符串值转换为布尔值，用于 RadioButton 的绑定。
/// ConverterParameter 指定要匹配的字符串值。
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        var stringValue = value.ToString() ?? string.Empty;
        var targetValue = parameter.ToString() ?? string.Empty;

        return stringValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value && parameter != null)
        {
            return parameter.ToString() ?? string.Empty;
        }

        return string.Empty;
    }
}
