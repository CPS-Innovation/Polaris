using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class AddDocumentNoteValidator : AbstractValidator<AddDocumentNoteRequestDto>
    {
        public AddDocumentNoteValidator()
        {
            RuleFor(x => x.DocumentId).GreaterThan(0).WithMessage("A valid document ID must be provided.");
            RuleFor(x => x.Text).NotEmpty().WithMessage("Note text must be provided.");
            RuleFor(x => x.Text).MaximumLength(500).WithMessage("The note text has a character limit of 500.");
        }
    }
}