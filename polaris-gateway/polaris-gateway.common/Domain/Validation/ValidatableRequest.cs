using System.Collections.Generic;
using FluentValidation.Results;

namespace PolarisGateway.Domain.Validation
{
    public class ValidatableRequest<T>
    {
        public T Value { get; set; }

        public bool IsValid { get; set; }

        public IList<ValidationFailure> Errors { get; set; }
    }
}
