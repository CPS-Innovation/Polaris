using Common.Dto.Request;
using FluentValidation;

namespace coordinator.Validators
{
    public class RenameDocumentValidator : AbstractValidator<RenameDocumentDto>
    {
        public RenameDocumentValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty();
            RuleFor(x => x.DocumentName).NotEmpty().MaximumLength(255);
        }
    }
}