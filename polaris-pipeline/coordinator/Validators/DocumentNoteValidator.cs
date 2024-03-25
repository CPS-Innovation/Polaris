using Common.Dto.Request;
using FluentValidation;

namespace coordinator.Validators
{
    public class DocumentNoteValidator : AbstractValidator<AddDocumentNoteDto>
    {
        public DocumentNoteValidator()
        {
            RuleFor(x => x.Text).NotEmpty();
            RuleFor(x => x.Text).MaximumLength(500);
        }
    }
}