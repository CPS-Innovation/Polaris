using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Common.Wrappers.Contracts
{
    public interface IValidatorWrapper<in TRequest>
    {
        ICollection<ValidationResult> Validate(TRequest request);
    }
}
