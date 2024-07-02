using Common.Dto.Request.DocumentManipulation;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class DocumentChangeValidator : AbstractValidator<DocumentChangesDto>
    {
        public DocumentChangeValidator()
        {
            RuleFor(x => x.PageIndex).NotEmpty().WithMessage("A page must be selected");
            RuleFor(x => x.Operation).NotEmpty();
            RuleFor(x => x.Arg).NotEmpty();
        }
    }
}