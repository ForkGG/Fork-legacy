using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Fork.View.Xaml.Converter;

[ValueConversion(typeof(double), typeof(Brush))]
internal class PercentageToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(Brush))
        {
            throw new ArgumentException("Target has to be Brush");
        }

        double percentage = (double)value;
        if (percentage <= 0.0)
        {
            return (Brush)new BrushConverter().ConvertFromString("#565B7A");
        }

        if (percentage < 10)
        {
            return (Brush)new BrushConverter().ConvertFromString("#3FA63F");
        }

        if (percentage < 20)
        {
            return (Brush)new BrushConverter().ConvertFromString("#68A63F");
        }

        if (percentage < 30)
        {
            return (Brush)new BrushConverter().ConvertFromString("#7DA63F");
        }

        if (percentage < 40)
        {
            return (Brush)new BrushConverter().ConvertFromString("#92A63F");
        }

        if (percentage < 50)
        {
            return (Brush)new BrushConverter().ConvertFromString("#A6A63F");
        }

        if (percentage < 60)
        {
            return (Brush)new BrushConverter().ConvertFromString("#A6923F");
        }

        if (percentage < 70)
        {
            return (Brush)new BrushConverter().ConvertFromString("#A67D3F");
        }

        if (percentage < 80)
        {
            return (Brush)new BrushConverter().ConvertFromString("#A6683F");
        }

        if (percentage < 90)
        {
            return (Brush)new BrushConverter().ConvertFromString("#A6543F");
        }

        if (percentage >= 90)
        {
            return (Brush)new BrushConverter().ConvertFromString("#A63F3F");
        }

        throw new ArgumentException("Percentage was not between 0 and 100");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}