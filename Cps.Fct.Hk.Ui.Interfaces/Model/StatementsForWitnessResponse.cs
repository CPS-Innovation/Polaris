// <copyright file="StatementsForWitnessResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Statements for witness response model.
/// </summary>
public record StatementsForWitnessResponse
{
    /// <summary>
    /// Gets or sets statements for witness response.
    /// </summary>
    [JsonPropertyName("statementsForWitness")]
    public List<StatementForWitness>? StatementsForWitness { get; set; }
}

/// <summary>
/// Details of a statement for witness.
/// </summary>
/// <param name="Id">The ID of the statement.</param>
/// <param name="StatementNumber">The statement number.</param>
public record StatementForWitness(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] int StatementNumber);
