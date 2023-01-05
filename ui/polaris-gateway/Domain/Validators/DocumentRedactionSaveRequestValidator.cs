using FluentValidation;
using RumpoleGateway.Domain.DocumentRedaction;

namespace RumpoleGateway.Domain.Validators
{
    public class DocumentRedactionSaveRequestValidator : AbstractValidator<DocumentRedactionSaveRequest>
    {
        public DocumentRedactionSaveRequestValidator()
        {
            RuleFor(x => x.Redactions).NotEmpty().WithMessage("At least one redaction must be provided");

            RuleForEach(c => c.Redactions).SetValidator(new RedactionValidator());
        }
    }
}
