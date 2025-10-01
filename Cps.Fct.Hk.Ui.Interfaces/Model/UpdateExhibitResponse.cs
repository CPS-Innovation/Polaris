// <copyright file="UpdateExhibitResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after update exhibit request.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateExhibitResponse"/> class.
/// </remarks>
/// <param name="updateExhibitData">The details of the updated exhibit. Can be null if no material is returned.</param>
public class UpdateExhibitResponse(UpdateExhibitData? updateExhibitData)
{
    /// <summary>
    /// Gets or sets the details of the updated material.
    /// </summary>
    [JsonPropertyName("updateExhibit")]
    public UpdateExhibitData? UpdateExhibitData { get; set; } = updateExhibitData;
}

/// <summary>
/// Represents the details of the updated material response data.
/// </summary>
public class UpdateExhibitData
{
    /// <summary>
    /// Gets or sets the material id of the updated material.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
