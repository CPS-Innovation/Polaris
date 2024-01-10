using FluentValidation;
using polaris_common.Dto.Request.Redaction;

namespace polaris_common.Domain.Validators
{
    public class RedactionCoordinatesValidator : AbstractValidator<RedactionCoordinatesDto>
    {
        public RedactionCoordinatesValidator()
        {
            RuleFor(x => x.X1).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Y1).GreaterThanOrEqualTo(0);
            RuleFor(x => x.X2).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Y2).GreaterThanOrEqualTo(0);
        }
    }
}
