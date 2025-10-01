// <copyright file="DiscardMaterialResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after a discard material request.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DiscardMaterialResponse"/> class.
/// </remarks>
/// <param name="discardMaterialData">The details of the discarded material. Can be null if no material is returned.</param>
public class DiscardMaterialResponse(DiscardMaterialData? discardMaterialData)
{
    /// <summary>
    /// Gets or sets the details of the discarded material.
    /// </summary>
    [JsonPropertyName("discardMaterial")]
    public DiscardMaterialData? DiscardMaterialData { get; set; } = discardMaterialData;
}

/// <summary>
/// Represents the details of the discarded material response data.
/// </summary>
public class DiscardMaterialData
{
    /// <summary>
    /// Gets or sets the material id of the renamed material.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
