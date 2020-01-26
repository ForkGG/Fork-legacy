using System;
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
            string returnString = String.Join("\r\n",(ObservableCollection<string>)value);

            return returnString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}