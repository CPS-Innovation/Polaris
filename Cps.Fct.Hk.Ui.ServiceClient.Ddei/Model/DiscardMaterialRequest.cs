// <copyright file="DiscardMaterialRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a request to discard a material.
/// </summary>
/// <param name="id"></param>
/// <param name="materialId"></param>
/// <param name="discardReason"></param>
/// <param name="discardReasonDescription"></param>
public record DiscardMaterialRequest(
    Guid id,
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("discardReason")] string discardReason,
    [property: JsonPropertyName("discardReasonDescription")] string discardReasonDescription)
    : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";
}
