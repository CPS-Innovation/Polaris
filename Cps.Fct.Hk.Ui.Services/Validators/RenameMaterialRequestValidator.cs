// <copyright file="RenameMaterialRequestValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Validators;

using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Cps.Fct.Hk.Ui.Services.Constants;
using FluentValidation;

/// <summary>
/// RenameMaterialRequest validator.
/// </summary>
public class RenameMaterialRequestValidator : AbstractValidator<RenameMaterialRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameMaterialRequestValidator"/> class.
    /// </summary>
    public RenameMaterialRequestValidator()
    {
        this.RuleFor(x => x.materialId)
          .NotEmpty().WithMessage("{PropertyName} is required.")
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.subject)
            .NotEmpty().WithMessage("{PropertyName} is required.")
              .Matches(RegexExpressions.RenameMaterialSubjectRegex)
              .WithMessage("{PropertyName} has invalid characters.");
    }
}
