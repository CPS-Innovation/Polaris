using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class ModifyDocumentPagesValidator : AbstractValidator<DocumentModificationRequestDto>
    {
        public ModifyDocumentPagesValidator()
        {
            RuleFor(x => x.DocumentChanges).NotEmpty().WithMessage("At least one modification must be provided");
            RuleForEach(x => x.DocumentChanges).SetValidator(new DocumentChangeValidator());
        }
    }
}