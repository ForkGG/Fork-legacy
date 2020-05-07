using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace nihilus.View.Xaml.Converter
{
    [ValueConversion(typeof(ObservableCollection<string>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("Target of conversion must be string");
            }
            List<string> strings = new List<string>((ObservableCollection<string>)value);
            string returnString = String.Join("\n",strings);

            return returnString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}