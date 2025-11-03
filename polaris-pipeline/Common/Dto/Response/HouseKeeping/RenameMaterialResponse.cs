// <copyright file="RenameMaterialResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after a rename material request.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RenameMaterialResponse"/> class.
/// </remarks>
/// <param name="renameMaterialData">The details of the renamed material.</param>
public class RenameMaterialResponse(RenameMaterialData renameMaterialData)
{
    /// <summary>
    /// Gets or sets the details of the renamed material.
    /// </summary>
    [JsonPropertyName("updateCommunication")]
    public RenameMaterialData RenameMaterialData { get; set; } = renameMaterialData ?? throw new ArgumentNullException(nameof(renameMaterialData));
}

/// <summary>
/// Represents the details of the rename material response data.
/// </summary>
public class RenameMaterialData
{
    /// <summary>
    /// Gets or sets the material id of the renamed material.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
