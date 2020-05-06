using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace nihilus.View.Xaml.ValidationRules
{
    class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int port;
            if(!int.TryParse(value.ToString(),out port))
            {
                return new ValidationResult(false, "Not a numeric value");
            }
            if(port < 1024 || port > 49151)
            {
                return new ValidationResult(false, "Port out of range");
            }
            return ValidationResult.ValidResult;
        }
    }
}
