// <copyright file="EditStatementRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents update statement request.
/// </summary>
public record EditStatementRequest([property: JsonPropertyName("witnessId")] int WitnessId,
    [property: JsonPropertyName("materialId")] int MaterialId,
    [property: JsonPropertyName("statementDate")] DateOnly? StatementDate,
    [property: JsonPropertyName("statementNumber")] int StatementNumber,
    [property: JsonPropertyName("used")] bool Used);
