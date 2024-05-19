using System;
using System.Globalization;
using System.Windows.Data;

namespace Fork.View.Xaml.Converter;

[ValueConversion(typeof(bool), typeof(bool))]
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(bool) || value is not bool boolValue)
        {
            throw new ArgumentException("This converter can only convert bool to bool");
        }

        return !boolValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(bool) || value is not bool boolValue)
        {
            throw new ArgumentException("This converter can only convert bool to bool");
        }

        return !boolValue;
    }
}