// <copyright file="UpdateExhibitRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;
using System.Text.Json.Serialization;

public record UpdateExhibitRequest(
    Guid id,
    int caseIdentifier,
    [property: JsonPropertyName("documentType")] int DocumentType,
    [property: JsonPropertyName("item")] string Item,
    int materialIdentifier,
    [property: JsonPropertyName("reference")] string Reference,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("used")] bool Used,
    [property: JsonPropertyName("newProducer")] string NewProducer,
    [property: JsonPropertyName("existingProducerOrWitnessId")] int? ExistingProducerOrWitnessId)
     : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";

    [JsonPropertyName("caseId")]
    public int CaseId { get; set; } = caseIdentifier;

    [JsonPropertyName("materialId")]
    public int MaterialId { get; set; } = materialIdentifier;
}
