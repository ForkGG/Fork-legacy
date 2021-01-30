using System.Globalization;
using System.Windows.Controls;

namespace Fork.View.Xaml.ValidationRules
{
    public class TimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int hours, minutes;
            string[] splittedTime = value.ToString().Split(':');
            if (splittedTime.Length != 2) return new ValidationResult(false, "Not a valid time format");
            if (!int.TryParse(splittedTime[0], out hours))
                return new ValidationResult(false, "Hours value is not numeric");
            if (!int.TryParse(splittedTime[1], out minutes))
                return new ValidationResult(false, "Minutes value is not numeric");
            if (hours < 0 || hours > 23) return new ValidationResult(false, "Hours value has to be between 0 and 23");
            if (minutes < 0 || minutes > 59)
                return new ValidationResult(false, "Minutes value has to be between 0 and 59");
            return ValidationResult.ValidResult;
        }
    }
}