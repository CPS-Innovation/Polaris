// <copyright file="UpdateStatementRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents update statement request.
/// </summary>
public record UpdateStatementRequest(
    Guid id,
    [property: JsonPropertyName("caseId")] int CaseId,
    int materialIdentifier,
    [property: JsonPropertyName("witnessId")] int WitnessId,
    [property: JsonPropertyName("statementDate")] DateOnly? StatementDate,
    [property: JsonPropertyName("statementNumber")] int StatementNumber,
    [property: JsonPropertyName("used")] bool Used)
       : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets or sets the material id.
    /// </summary>
    [JsonPropertyName("materialId")]
    public int MaterialId { get; set; } = materialIdentifier;

    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";

}
