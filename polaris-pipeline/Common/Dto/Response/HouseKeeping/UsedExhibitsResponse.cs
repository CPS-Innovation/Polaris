// <copyright file="UsedExhibitsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>
/// Represents the response received after a get used exhibits operation.
/// </summary>
public class UsedExhibitsResponse
{
    /// <summary>
    /// Gets or sets the list of used exhibits associated with the response.
    /// Each exhibit contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="Exhibit"/> objects that represent the used exhibits.</value>
    [JsonPropertyName("exhibits")]
    public List<Exhibit> Exhibits { get; set; }
}
