// <copyright file="ValidateObjectAttribute.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Validators
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class ValidateObjectAttribute : ValidationAttribute
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
