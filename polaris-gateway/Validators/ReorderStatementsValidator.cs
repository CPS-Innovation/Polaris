using Common.Dto.Case;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class ReorderStatementsValidator : AbstractValidator<OrderedStatementsDto>
    {
        public ReorderStatementsValidator()
        {
            RuleFor(x => x.Statements).NotEmpty();
            RuleForEach(x => x.Statements).SetValidator(new OrderedStatementValidator());
        }
    }
}