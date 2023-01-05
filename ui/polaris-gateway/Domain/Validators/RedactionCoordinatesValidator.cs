﻿using FluentValidation;
using RumpoleGateway.Domain.DocumentRedaction;

namespace RumpoleGateway.Domain.Validators
{
    public class RedactionCoordinatesValidator : AbstractValidator<RedactionCoordinates>
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
