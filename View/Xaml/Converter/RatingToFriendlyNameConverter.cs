using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Fork.Logic;
using Fork.Logic.Model.PluginModels;

namespace Fork.View.Xaml.Converter
{
    [ValueConversion(typeof(PluginEnums.Sorting), typeof(string))]
    public class RatingToFriendlyNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rating = (PluginEnums.Sorting) value;
            return rating.FriendlyName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PluginEnums.Sorting s)
            {
                return s.FriendlyName();
            }
            string friendly = value.ToString();
            switch (friendly)
            {
                case "Most Ratings": return PluginEnums.Sorting.RATING;
                case "Downloads": return PluginEnums.Sorting.DOWNLOADS;
                case "Last Updated": return PluginEnums.Sorting.LAST_UPDATE;
                case "Release Date": return PluginEnums.Sorting.SUBMISSION_DATE;
                default: return PluginEnums.Sorting.RATING;
            }
        }
    }
}