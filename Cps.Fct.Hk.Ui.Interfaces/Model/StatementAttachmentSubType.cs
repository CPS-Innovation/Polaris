// <copyright file="StatementAttachmentSubType.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the sub-type for a statement attachment.
/// </summary>
/// <param name="WitnessName">The name of the witness.</param>
/// <param name="WitnessTitle">The title of the witness.</param>
/// <param name="WitnessShoulderNo">The shoulder number of the witness.</param>
/// <param name="StatementNo">The statement number.</param>
/// <param name="Date">The date of the statement.</param>
/// <param name="Witness">The ID of the witness.</param>
public record StatementAttachmentSubType(
    [property: JsonPropertyName("witnessName")] string? WitnessName,
    [property: JsonPropertyName("witnessTitle")] string? WitnessTitle,
    [property: JsonPropertyName("witnessShoulderNo")] string? WitnessShoulderNo,
    [property: JsonPropertyName("statementNo")] string? StatementNo,
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("witness")] int? Witness);
