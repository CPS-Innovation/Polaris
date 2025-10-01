// <copyright file="ReclassifyCaseMaterialRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a request to reclassify a communication.
/// </summary>
/// <param name="classification">The classification of the communication.</param>
/// <param name="materialId">The material ID associated with the communication.</param>
/// <param name="documentTypeId">The document type ID associated with the communication.</param>
/// <param name="used">Indicates whether the material has been used.</param>
/// <param name="subject">The subject of the communication.</param>
/// <param name="urn">The urn of the case.</param>
/// <param name="statementRequest">The case material statement if any.</param>
/// <param name="exhibitRequest">The case material exhibit if any.</param>
public record ReclassifyCaseMaterialRequest(
    [property: JsonPropertyName("urn")] string urn,
    [property: JsonPropertyName("classification")] string classification,
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("documentTypeId")] int documentTypeId,
    [property: JsonPropertyName("used")] bool used,
    [property: JsonPropertyName("subject")] string subject,
    ReclassifyStatementRequest? statementRequest = null,
    ReclassifyExhibitRequest? exhibitRequest = null)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";

    /// <summary>
    /// Gets the case material statement.
    /// </summary>
    [JsonPropertyName("statement")]
    public ReclassifyStatementRequest? Statement { get; init; } = statementRequest;

    /// <summary>
    /// Gets the case material exhibit.
    /// </summary>
    [JsonPropertyName("exhibit")]
    public ReclassifyExhibitRequest? Exhibit { get; init; } = exhibitRequest;
}
