using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class ReclassifyDocumentValidator : AbstractValidator<ReclassifyDocumentDto>
    {
        public ReclassifyDocumentValidator()
        {
            RuleFor(x => x.DocumentTypeId).NotEmpty();
            When(x => x.Exhibit != null, () =>
            {
                RuleFor(x => x.Statement).Empty();
                RuleFor(x => x.Other).Empty();
                RuleFor(x => x.Immediate).Empty();
                RuleFor(x => x.Exhibit.Item).NotEmpty();
                RuleFor(x => x.Exhibit.Reference).NotEmpty();
            });
            When(x => x.Statement != null, () =>
            {
                RuleFor(x => x.Exhibit).Empty();
                RuleFor(x => x.Other).Empty();
                RuleFor(x => x.Immediate).Empty();
                RuleFor(x => x.Statement.WitnessId).NotEmpty();
                RuleFor(x => x.Statement.StatementNo).NotEmpty();
                When(x => !string.IsNullOrEmpty(x.Statement.Date), () =>
                {
                    RuleFor(x => x.Statement.Date).Must(BeAValidDate);
                });
            });
            When(x => x.Other != null, () =>
            {
                RuleFor(x => x.Exhibit).Empty();
                RuleFor(x => x.Statement).Empty();
                RuleFor(x => x.Immediate).Empty();
                When(x => x.Other.DocumentName != null, () =>
                {
                    RuleFor(x => x.Other.DocumentName).MaximumLength(255);
                });
            });
            When(x => x.Immediate != null, () =>
            {
                RuleFor(x => x.Exhibit).Empty();
                RuleFor(x => x.Statement).Empty();
                RuleFor(x => x.Other).Empty();
                When(x => x.Immediate.DocumentName != null, () =>
                {
                    RuleFor(x => x.Immediate.DocumentName).MaximumLength(255);
                });
            });
        }

        private static bool BeAValidDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date);
        }
    }
}