// <copyright file="RenameMaterialRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a request to rename a material.
/// </summary>
/// <param name="id"></param>
/// <param name="materialId"></param>
/// <param name="subject"></param>
public record RenameMaterialRequest(
    Guid id,
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("subject")] string subject)
    : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";
}
