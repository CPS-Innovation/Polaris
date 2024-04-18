using Common.Dto.Request;
using FluentValidation;

namespace Common.Domain.Validators
{
  public class RedactPdfRequestWithDocumentValidator : AbstractValidator<RedactPdfRequestWithDocumentDto>
  {
    public RedactPdfRequestWithDocumentValidator()
    {
      RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the redaction request");
      RuleFor(x => x.RedactionDefinitions).NotEmpty().WithMessage("At least one redaction definition must be provided");
      RuleFor(x => x.Document).NotEmpty().WithMessage("No document was included in the redaction request");
      RuleForEach(x => x.RedactionDefinitions).SetValidator(new RedactionDefinitionValidator());
    }
  }
}
