// <copyright file="DefendantsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Response received following a get defendants operation.
/// </summary>
public record DefendantsResponse
{
    /// <summary>
    /// Gets or sets the list of defendants received when get operation is performed.
    /// </summary>
    [JsonPropertyName("defendants")]
    public List<Defendant> Defendants { get; set; }
}
