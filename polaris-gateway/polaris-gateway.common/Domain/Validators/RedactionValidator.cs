using Common.Dto.Request.Redaction;
using FluentValidation;

namespace PolarisGateway.Domain.Validators
{
    public class RedactionValidator : AbstractValidator<RedactionDefinitionDto>
    {
        public RedactionValidator()
        {
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Width).GreaterThan(0);
            RuleFor(x => x.Height).GreaterThan(0);
            RuleFor(x => x.RedactionCoordinates).NotEmpty();
            RuleForEach(x => x.RedactionCoordinates).SetValidator(new RedactionCoordinatesValidator());
        }
    }
}
