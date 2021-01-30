using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fork.Logic.Model;

namespace Fork.View.Xaml.Converter
{
    [ValueConversion(typeof(ServerStatus), typeof(Visibility))]
    public class EntityStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new ArgumentException("This converter can only convert bools to Visibility");

            if (value != null && ((ServerStatus) value == ServerStatus.RUNNING ||
                                  (ServerStatus) value == ServerStatus.STARTING)) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseEntityStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new ArgumentException("This converter can only convert bools to Visibility");

            if (value != null && (ServerStatus) value == ServerStatus.STOPPED) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}