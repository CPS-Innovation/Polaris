// <copyright file="ReclassifyCommunicationRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a request to reclassify a case material.
/// </summary>
/// <param name="correspondenceId">The unique identifier for the correspondence.</param>
/// <param name="classification">The classification of the communication.</param>
/// <param name="materialId">The material ID associated with the communication.</param>
/// <param name="documentTypeId">The document type ID associated with the communication.</param>
/// <param name="used">Indicates whether the material has been used.</param>
/// <param name="subject">The subject of the communication.</param>
public record ReclassifyCommunicationRequest(
    Guid id,
    [property: JsonPropertyName("classification")] string classification,
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("documentTypeId")] int documentTypeId,
    [property: JsonPropertyName("used")] bool used,
    [property: JsonPropertyName("subject")] string subject,
    ReclassifyStatementRequest? statementRequest = null,
    ReclassifyExhibitRequest? exhibitRequest = null)
    : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";

    [JsonPropertyName("statement")]
    public ReclassifyStatementRequest? Statement { get; init; } = statementRequest;

    [JsonPropertyName("exhibit")]
    public ReclassifyExhibitRequest? Exhibit { get; init; } = exhibitRequest;
}
