﻿using Common.Domain.Requests;
using FluentValidation;

namespace pdf_generator.Domain.Validators
{
    public class RedactPdfRequestValidator : AbstractValidator<RedactPdfRequest>
    {
        public RedactPdfRequestValidator()
        {
            RuleFor(x => x.CaseId).NotEmpty().WithMessage("An invalid case Id was supplied");
            RuleFor(x => x.DocumentId).NotEmpty().WithMessage("An invalid document Id was provided");
            RuleFor(x => x.FileName).NotEmpty().WithMessage("No filename was included in the redaction request");
            RuleFor(x => x.RedactionDefinitions).NotEmpty().WithMessage("At least one redaction definition must be provided");
            RuleForEach(x => x.RedactionDefinitions).SetValidator(new RedactionDefinitionValidator());
        }
    }
}
