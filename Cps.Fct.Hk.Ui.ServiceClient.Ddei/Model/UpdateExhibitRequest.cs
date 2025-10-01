// <copyright file="UpdateExhibitRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;
using System.Text.Json.Serialization;

public record UpdateExhibitRequest(
    Guid id,
    [property: JsonPropertyName("caseId")] int CaseId,
    [property: JsonPropertyName("documentType")] int DocumentType,
    [property: JsonPropertyName("item")] string Item,
    [property: JsonPropertyName("materialId")] int MaterialId,
    [property: JsonPropertyName("reference")] string Reference,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("used")] bool Used,
    [property: JsonPropertyName("newProducer")] string? NewProducer,
    [property: JsonPropertyName("existingProducerOrWitnessId")] int? ExistingProducerOrWitnessId)
     : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";
}
