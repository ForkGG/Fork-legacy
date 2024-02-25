using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Fork.View.Xaml.Converter;

[ValueConversion(typeof(ObservableCollection<string>), typeof(string))]
public class ListToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(string))
        {
            throw new InvalidOperationException("Target of conversion must be string");
        }

        ObservableCollection<string> valueCol = value as ObservableCollection<string>;
        try
        {
            List<string> strings = valueCol?.ToList();

            if (strings == null)
            {
                return "";
            }

            return string.Join("\n", strings);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(
                "Argument Exception while Converting List to String (List probably changed while converting)");
            return "";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(ObservableCollection<string>))
        {
            throw new InvalidOperationException("Target of conversion must be ObservableCollection<string>");
        }

        string valueString = (string)value;
        List<string> strings = new(valueString.Split('\n'));
        List<string> cleanStrings = new();
        foreach (string s in strings) cleanStrings.Add(s.Replace("\n", "").Replace("\r", ""));
        return new ObservableCollection<string>(cleanStrings);
    }
}