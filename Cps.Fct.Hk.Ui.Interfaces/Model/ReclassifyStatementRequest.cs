// <copyright file="ReclassifyStatementRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System;

/// <summary>
/// Represents a reclassification statement model.
/// </summary>
public record ReclassifyStatementRequest
{
    /// <summary>
    /// Gets or sets the unique ID of  the witness.
    /// </summary>
    public int Witness { get; set; }

    /// <summary>
    /// Gets or sets the statement number.
    /// </summary>
    public int StatementNo { get; set; }

    /// <summary>
    /// Gets or sets the received date.
    /// </summary>
    public DateOnly? Date { get; set; } = default(DateOnly);
}
