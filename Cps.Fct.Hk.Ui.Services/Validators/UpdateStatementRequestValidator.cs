// <copyright file="UpdateStatementRequestValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Validators;

using Common.Dto.Request.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces.Model;

using FluentValidation;

/// <summary>
/// UpdateStatementRequest validator.
/// </summary>
public class UpdateStatementRequestValidator : AbstractValidator<UpdateStatementRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStatementRequestValidator"/> class.
    /// </summary>
    public UpdateStatementRequestValidator()
    {
        this.RuleFor(x => x.MaterialId)
       .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.WitnessId)
              .NotEmpty().WithMessage("{PropertyName} is required.")
              .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.StatementNumber)
              .NotEmpty().WithMessage("{PropertyName} is required.")
              .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

        this.RuleFor(x => x.Used)
               .NotNull().WithMessage("{PropertyName} is required.");
    }
}
