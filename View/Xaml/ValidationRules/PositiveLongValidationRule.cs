using System.Windows.Controls;

namespace Fork.View.Xaml.ValidationRules
{
    public class PositiveLongValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            long val;
            if(!long.TryParse(value.ToString(),out val))
            {
                return new ValidationResult(false, "Not a numeric value");
            }
            return val < 0 ? new ValidationResult(false, "Value out of range") : ValidationResult.ValidResult;
        }
    }
}