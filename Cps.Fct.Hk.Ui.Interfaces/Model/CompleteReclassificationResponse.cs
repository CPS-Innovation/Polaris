// <copyright file="CompleteReclassificationResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a complete reclassification response.
/// </summary>
public record CompleteReclassificationResponse(
    [property: JsonPropertyName("overallSuccess")] bool overallSuccess,
    [property: JsonPropertyName("status")] string status, // "Success", "Failed", "PartialSuccess"
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("transactionId")] string transactionId,
    [property: JsonPropertyName("reclassificationResult")] OperationResult reclassificationResult,
    [property: JsonPropertyName("renameMaterialResult")] OperationResult? renameMaterialResult,
    [property: JsonPropertyName("actionPlanResult")] OperationResult? actionPlanResult,
    [property: JsonPropertyName("witnessResult")] OperationResult? addWitnessResult,
    [property: JsonPropertyName("errors")] string[] errors,
    [property: JsonPropertyName("warnings")] string[]? warnings = null)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";
}
