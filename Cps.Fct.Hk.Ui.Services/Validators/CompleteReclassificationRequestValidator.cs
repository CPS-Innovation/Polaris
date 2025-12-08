// <copyright file="CompleteReclassificationRequestValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Validators;

using System;
using Common.Dto.Request.HouseKeeping;
using Cps.Fct.Hk.Ui.Services.Constants;
using FluentValidation;

/// <summary>
/// Validates complete reclassification request.
/// </summary>
public class CompleteReclassificationRequestValidator : AbstractValidator<CompleteReclassificationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteReclassificationRequestValidator"/> class.
    /// </summary>
    public CompleteReclassificationRequestValidator()
    {
        this.RuleFor(x => x.reclassification.subject)
           .NotEmpty().WithMessage("{PropertyName} is required.")
             .Matches(RegexExpressions.RenameMaterialSubjectRegex)
             .WithMessage("{PropertyName} has invalid characters.");

        this.RuleFor(x => x.reclassification.classification)
         .NotEmpty().WithMessage("{PropertyName} is required.");

        this.RuleFor(x => x.reclassification.documentTypeId)
           .NotEmpty().WithMessage("{PropertyName} is required")
             .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.reclassification.Statement)
             .NotEmpty()
             .When(x => x.reclassification.classification != null &&
                        x.reclassification.classification.Equals("statement", StringComparison.InvariantCultureIgnoreCase))
             .WithMessage("Statement body must be provided.");

        this.RuleFor(x => x.reclassification.Exhibit)
             .NotEmpty()
             .When(x => x.reclassification.classification != null &&
                        x.reclassification.classification.Equals("exhibit", StringComparison.InvariantCultureIgnoreCase))
             .WithMessage("Exhibit body must be provided.");
    }
}
