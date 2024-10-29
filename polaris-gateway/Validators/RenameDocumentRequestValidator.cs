using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class RenameDocumentRequestValidator : AbstractValidator<RenameDocumentRequestDto>
    {
        public RenameDocumentRequestValidator()
        {
            RuleFor(x => x.DocumentName).NotEmpty().WithMessage("A document name is required").MaximumLength(255);
        }
    }
}