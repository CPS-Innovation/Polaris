using System;
using Common.Dto.Request;
using FluentValidation;

namespace coordinator.Validators
{
    public class ReclassifyDocumentValidator : AbstractValidator<ReclassifyDocumentDto>
    {
        public ReclassifyDocumentValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty();
            RuleFor(x => x.DocumentTypeId).NotEmpty();
            When(x => x.ReclassificationType == ReclassificationType.Exhibit, () =>
            {
                RuleFor(x => x.Exhibit.Item).NotEmpty();
                RuleFor(x => x.Exhibit.Reference).NotEmpty();
            });
            When(x => x.ReclassificationType == ReclassificationType.Statement, () =>
            {
                RuleFor(x => x.Statement.WitnessId).NotEmpty();
                RuleFor(x => x.Statement.StatementNo).NotEmpty();
                When(x => !string.IsNullOrEmpty(x.Statement.Date), () =>
                {
                    RuleFor(x => x.Statement.Date).Must(BeAValidDate);
                });
            });
        }

        private static bool BeAValidDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date);
        }
    }
}