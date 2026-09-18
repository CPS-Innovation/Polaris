using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Common.Validators
{
    public class LoggerObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(value, new ValidationContext(value), results, true);

            return results.Any() ?
                new ValidationResult(
                    string.Join(
                        Environment.NewLine,
                        results.Select(r => $"{validationContext.DisplayName}: {r}"))) :
                ValidationResult.Success;
        }
    }
}
