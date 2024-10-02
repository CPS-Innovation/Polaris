using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class DocumentRedactionSaveRequestValidator : AbstractValidator<DocumentRedactionSaveRequestDto>
    {
        public DocumentRedactionSaveRequestValidator()
        {
            When(x => x.DocumentModifications == null, () =>
            {
                RuleFor(x => x.Redactions).NotEmpty().WithMessage("At least one redaction must be provided");
            });
            When(x => x.Redactions == null, () =>
            {
                RuleFor(x => x.DocumentModifications).NotEmpty().WithMessage("At least one page deletion must be provided");
            });
            When(x => x.Redactions != null, () =>
            {
                RuleFor(x => x.Redactions).NotEmpty().WithMessage("At least one redaction must be provided");
                RuleForEach(c => c.Redactions).SetValidator(new RedactionValidator());
            });
        }
    }
}
