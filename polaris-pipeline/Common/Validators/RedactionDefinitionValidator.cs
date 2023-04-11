using Common.Domain.Validators;
using Common.Dto.Request.Redaction;
using FluentValidation;

namespace Cmmon.Domain.Validators
{
    public class RedactionDefinitionValidator : AbstractValidator<RedactionDefinitionDto>
    {
        public RedactionDefinitionValidator()
        {
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Width).GreaterThan(0);
            RuleFor(x => x.Height).GreaterThan(0);
            RuleFor(x => x.RedactionCoordinates).NotEmpty();
            RuleForEach(x => x.RedactionCoordinates).SetValidator(new RedactionCoordinatesValidator());
        }
    }
}
