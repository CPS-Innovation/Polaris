﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Common.Wrappers
{
    public class ValidatorWrapper<TRequest> : IValidatorWrapper<TRequest>
    {
        public ICollection<ValidationResult> Validate(TRequest request)
        {
            var validationResults = new Collection<ValidationResult>();
            if (request != null)
                Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
            else
            {
                validationResults.Add(new ValidationResult("A null request was received and could not be validated."));
            }
            return validationResults;
        } 
    }
}
