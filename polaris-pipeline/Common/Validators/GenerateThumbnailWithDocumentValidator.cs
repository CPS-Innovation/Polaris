using Common.Dto.Request;
using FluentValidation;

namespace Common.Domain.Validators
{
  public class GenerateThumbnailWithDocumentValidator : AbstractValidator<GenerateThumbnailWithDocumentDto>
  {
    public GenerateThumbnailWithDocumentValidator()
    {
      RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the request");
      RuleFor(x => x.Document).NotEmpty().WithMessage("No document was included in the request");
    }
  }
}