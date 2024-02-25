using System;
using System.Globalization;
using System.Windows.Data;
using Fork.Annotations;

namespace Fork.View.Xaml.Converter;

public class ComparisonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.Equals(true) == true ? parameter : Binding.DoNothing;
    }
}