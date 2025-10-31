// <copyright file="PCDPoliceContactDetails.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping.Pcd;

using System.Text.Json.Serialization;

/// <summary>
/// Pcd request contact details.
/// </summary>
public class PCDPoliceContactDetails
{
    /// <summary>
    /// Gets or sets the Pcd Police contact role.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    /// Gets or sets the Pcd Police contact rank.
    /// </summary>
    [JsonPropertyName("rank")]
    public string Rank { get; set; }

    /// <summary>
    /// Gets or sets the Pcd Police contact name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request Police contact number.
    /// </summary>
    [JsonPropertyName("number")]
    public string Number { get; set; }
}
