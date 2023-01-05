using FluentValidation;

namespace Common.tests.Wrappers
{
    public class ValidatableStubRequest
    {
        public string StubString { get; set; } = string.Empty;
    }

    public class ValidatableStubRequestValidator : AbstractValidator<ValidatableStubRequest>
    {
        public ValidatableStubRequestValidator()
        {
            RuleFor(x => x.StubString).NotEmpty().WithMessage("StubString should not be empty");
        }
    }
}
