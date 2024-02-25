using System;
using System.Globalization;
using System.Windows.Data;
using Fork.Logic.Model;

namespace Fork.View.Xaml.Converter;

[ValueConversion(typeof(SimpleTime), typeof(string))]
public class SimpleTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(string))
        {
            throw new InvalidOperationException("Target of conversion must be string");
        }

        SimpleTime time = value as SimpleTime;
        if (time == null)
        {
            throw new ArgumentException("Value has to be SimpleTime");
        }

        return time.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(SimpleTime))
        {
            throw new InvalidOperationException("Target of conversion must be SimpleTime");
        }

        string time = value as string;
        if (time == null)
        {
            throw new ArgumentException("Value has to be string");
        }

        string[] times = time.Split(':');
        return new SimpleTime(int.Parse(times[0]), int.Parse(times[1]));
    }
}