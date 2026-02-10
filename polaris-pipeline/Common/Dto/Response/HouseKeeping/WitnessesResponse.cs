// <copyright file="WitnessesResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Response received following a get witnesses operation.
/// </summary>
public record WitnessesResponse
{
    /// <summary>
    /// Gets or sets the list of witnesses received when get operation is performed.
    /// </summary>
    [JsonPropertyName("witnesses")]
    public List<Witness> Witnesses { get; set; }
}
