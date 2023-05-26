using Cmmon.Domain.Validators;
using Common.Dto.Request;
using FluentValidation;

namespace Common.Domain.Validators
{
    public class RedactPdfRequestValidator : AbstractValidator<RedactPdfRequestDto>
    {
        public RedactPdfRequestValidator()
        {
            RuleFor(x => x.CaseId).NotEmpty().WithMessage("An invalid case Id was supplied");
            RuleFor(x => x.PolarisDocumentId.Value).NotEmpty().WithMessage("An invalid polaris document Id was provided");
            RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the redaction request");
            RuleFor(x => x.RedactionDefinitions).NotEmpty().WithMessage("At least one redaction definition must be provided");
            RuleForEach(x => x.RedactionDefinitions).SetValidator(new RedactionDefinitionValidator());
        }
    }
}
