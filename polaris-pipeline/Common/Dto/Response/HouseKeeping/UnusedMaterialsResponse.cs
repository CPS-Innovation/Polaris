// <copyright file="UnusedMaterialsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>
/// Represents the response received after a get unused materials operation.
/// </summary>
public class UnusedMaterialsResponse
{
    /// <summary>
    /// Gets or sets the list of unused exhibits associated with the response.
    /// Each exhibit contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="Exhibit"/> objects that represent the unused exhibits.</value>
    [JsonPropertyName("exhibits")]
    public List<Exhibit> Exhibits { get; set; }

    /// <summary>
    /// Gets or sets the list of unused mg forms associated with the response.
    /// Each mg form contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="MgForm"/> objects that represent the unused mg forms.</value>
    [JsonPropertyName("mgForms")]
    public List<MgForm> MgForms { get; set; }

    /// <summary>
    /// Gets or sets the list of unused other materials associated with the response.
    /// Each other material contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="MgForm"/> objects that represent the unused other material.</value>
    [JsonPropertyName("otherMaterials")]
    public List<MgForm> OtherMaterials { get; set; }

    /// <summary>
    /// Gets or sets the list of unused statements associated with the response.
    /// Each statement contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="Statement"/> objects that represent the unused statements.</value>
    [JsonPropertyName("statements")]
    public List<Statement> Statements { get; set; }
}
