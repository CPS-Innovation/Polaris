using System.ComponentModel.DataAnnotations;

namespace polaris_common.Wrappers.Contracts
{
    public interface IValidatorWrapper<in TRequest>
    {
        ICollection<ValidationResult> Validate(TRequest request);
    }
}
