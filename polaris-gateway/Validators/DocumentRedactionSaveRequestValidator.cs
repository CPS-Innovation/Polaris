﻿using Common.Dto.Request;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class DocumentRedactionSaveRequestValidator : AbstractValidator<DocumentRedactionSaveRequestDto>
    {
        public DocumentRedactionSaveRequestValidator()
        {
            When(x => x.DocumentModifications == null || !x.DocumentModifications.Any(), () =>
            {
                RuleFor(x => x.Redactions).NotEmpty().WithMessage("At least one redaction must be provided");
            });
            When(x => x.Redactions == null || !x.Redactions.Any(), () =>
            {
                RuleFor(x => x.DocumentModifications).NotEmpty().WithMessage("At least one page deletion must be provided");
            });
            When(x => x.Redactions != null && x.Redactions.Any(), () =>
            {
                RuleForEach(c => c.Redactions).SetValidator(new RedactionValidator());
            });
        }
    }
}
