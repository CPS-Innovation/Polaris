// <copyright file="UpdateExhibitRequestValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Validators;

using Cps.Fct.Hk.Ui.Interfaces.Model;
using FluentValidation;

/// <summary>
/// Edit exhibit request validator.
/// </summary>
public class UpdateExhibitRequestValidator : AbstractValidator<EditExhibitRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateExhibitRequestValidator"/> class.
    /// </summary>
    public UpdateExhibitRequestValidator()
    {
        this.RuleFor(x => x.MaterialId)
       .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.Item)
                .NotEmpty().WithMessage("{PropertyName} is required.");

        this.RuleFor(x => x.Reference)
                .NotEmpty().WithMessage("{PropertyName} is required.");

        this.RuleFor(x => x.DocumentType)
             .NotEmpty().WithMessage("{PropertyName} is required.")
              .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.Used)
                .NotNull().WithMessage("{PropertyName} is required.");
    }
}
