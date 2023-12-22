using System.Collections.Generic;
using FluentValidation.Results;

namespace Common.Domain.Validation
{
    public class ValidatableRequest<T>
    {
        public T Value { get; set; }

        public bool IsValid { get; set; }

        public IList<ValidationFailure> Errors { get; set; }

        public string RequestJson { get; set; }
    }
}
