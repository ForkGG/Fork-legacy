using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using nihilus.Logic.Model;

namespace nihilus.View.Xaml.Converter
{
    [ValueConversion(typeof(double), typeof(Brush))]
    class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new ArgumentException("Target has to be Brush");
            }

            double percentage = (double)value;
            if(percentage <= 1)
            {
                return (Brush)new BrushConverter().ConvertFromString("#565B7A");
            }
            if (percentage < 10)
            {
                return (Brush)new BrushConverter().ConvertFromString("#00FF00");
            }
            if (percentage < 20)
            {
                return (Brush)new BrushConverter().ConvertFromString("#66ff00");
            }
            if (percentage < 30)
            {
                return (Brush)new BrushConverter().ConvertFromString("#99ff00");
            }
            if (percentage < 40)
            {
                return (Brush)new BrushConverter().ConvertFromString("#ccff00");
            }
            if (percentage < 50)
            {
                return (Brush)new BrushConverter().ConvertFromString("#FFFF00");
            }
            if (percentage < 60)
            {
                return (Brush)new BrushConverter().ConvertFromString("#FFCC00");
            }
            if (percentage < 70)
            {
                return (Brush)new BrushConverter().ConvertFromString("#ff9900");
            }
            if (percentage < 80)
            {
                return (Brush)new BrushConverter().ConvertFromString("#ff6600");
            }
            if (percentage < 90)
            {
                return (Brush)new BrushConverter().ConvertFromString("#FF3300");
            }
            if (percentage<=100)
            {
                return (Brush)new BrushConverter().ConvertFromString("#FF0000");
            }

            throw new ArgumentException("Percentage was not between 0 and 100");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
