// <copyright file="UsedOtherMaterialsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>
/// Represents the response received after a get used other materials operation.
/// </summary>
public class UsedOtherMaterialsResponse
{
    /// <summary>
    /// Gets or sets the list of used other materials associated with the response.
    /// Each other material contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="MgForm"/> objects that represent the used other material.</value>
    [JsonPropertyName("mgForms")]
    public List<MgForm> MgForms { get; set; }
}
