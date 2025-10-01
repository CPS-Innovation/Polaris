// <copyright file="MaterialProvided.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model.PCD;

using System.Text.Json.Serialization;

/// <summary>
/// Pcd request material provided info.
/// </summary>
public class MaterialProvided
{
    /// <summary>
    /// Gets or sets the Pcd request Material Provided subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request Material Provided date.
    /// </summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }
}
