using Common.Dto.Request;
using FluentValidation;

namespace pdf_redactor.Domain.Validators
{
    public class RedactPdfRequestValidator : AbstractValidator<RedactPdfRequestDto>
    {
        public RedactPdfRequestValidator()
        {
            RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the redaction request");
            RuleFor(x => x.RedactionDefinitions).NotEmpty().WithMessage("At least one redaction definition must be provided");
            RuleForEach(x => x.RedactionDefinitions).SetValidator(new RedactionDefinitionValidator());
        }
    }
}
