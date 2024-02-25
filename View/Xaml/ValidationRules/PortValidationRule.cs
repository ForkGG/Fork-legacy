using System.Globalization;
using System.Windows.Controls;

namespace Fork.View.Xaml.ValidationRules;

internal class PortValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        int port;
        if (!int.TryParse(value.ToString(), out port))
        {
            return new ValidationResult(false, "Not a numeric value");
        }

        if (port < 1024 || port > 49151)
        {
            return new ValidationResult(false, "Port out of range");
        }

        return ValidationResult.ValidResult;
    }
}