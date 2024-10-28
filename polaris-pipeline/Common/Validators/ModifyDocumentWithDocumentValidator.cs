using Common.Dto.Request;
using FluentValidation;

namespace Common.Domain.Validators
{
    public class ModifyDocumentWithDocumentValidator : AbstractValidator<ModifyDocumentWithDocumentDto>
    {
        public ModifyDocumentWithDocumentValidator()
        {
            RuleFor(x => x.DocumentModifications).NotEmpty().WithMessage("You must select at least one page to modify");
            RuleFor(x => x.Document).NotEmpty().WithMessage("No document was included in the request");
        }
    }
}