using Common.Dto.Case;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class OrderedStatementValidator : AbstractValidator<OrderedStatementDto>
    {
        public OrderedStatementValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty();
            RuleFor(x => x.ListOrder).NotEmpty().GreaterThan(0);
        }
    }
}