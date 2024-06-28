using Common.Dto.Request;
using FluentValidation;

namespace Common.Validators
{
    public class RemoveDocumentPagesWithDocumentValidator : AbstractValidator<RemoveDocumentPagesWithDocumentDto>
    {
        public RemoveDocumentPagesWithDocumentValidator()
        {
            RuleFor(x => x.PagesIndexesToRemove).NotEmpty().WithMessage("You must select at least one page to remove");
            RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the request");
            RuleFor(x => x.Document).NotEmpty().WithMessage("No document was included in the request");
        }
    }
}