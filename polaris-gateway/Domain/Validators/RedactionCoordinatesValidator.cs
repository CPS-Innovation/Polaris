using Common.Dto.Request.Redaction;
using FluentValidation;

namespace PolarisGateway.Domain.Validators
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
