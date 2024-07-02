using Common.Dto.Request;
using FluentValidation;

namespace Common.Validators
{
    public class RemoveDocumentPagesWithDocumentValidator : AbstractValidator<ModifyDocumentWithDocumentDto>
    {
        public RemoveDocumentPagesWithDocumentValidator()
        {
            RuleFor(x => x.DocumentChanges).NotEmpty().WithMessage("You must select at least one page to remove");
            RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the request");
            RuleFor(x => x.Document).NotEmpty().WithMessage("No document was included in the request");
        }
    }
}