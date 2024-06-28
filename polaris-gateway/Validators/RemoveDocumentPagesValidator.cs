using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class RemoveDocumentPagesValidator : AbstractValidator<DocumentPageRemovalRequestDto>
    {
        public RemoveDocumentPagesValidator()
        {
            RuleFor(x => x.PagesIndexesToRemove).NotEmpty().WithMessage("At least one page must be provided");
        }
    }
}