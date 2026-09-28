// <copyright file="CompleteReclassificationRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System.Text.Json.Serialization;

/// <summary>
/// Represents request for a complete reclassificaton of a case material.
/// </summary>
public record CompleteReclassificationRequest(
    [property: JsonPropertyName("reclassification")] ReclassifyCaseMaterialRequest reclassification,
    [property: JsonPropertyName("actionPlan")] AddCaseActionPlanRequest actionPlan,
    [property: JsonPropertyName("witness")] WitnessRequest? witness)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";

    /// <summary>
    /// Flag to indicate if request has statement.
    /// </summary>
    /// <returns>True if request has statement object, otherwise false.</returns>
    public bool HasStatement()
    {
        return reclassification?.Statement != null;
    }

    /// <summary>
    /// Flag to indicate if request has exhibit.
    /// </summary>
    /// <returns>True if request has exhibit object, otherwise false.</returns>
    public bool HasExhibit()
    {
        return reclassification?.Exhibit != null;
    }
}
