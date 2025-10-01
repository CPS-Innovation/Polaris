// <copyright file="PcdRequestCore.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model.PCD;

using System.Text.Json.Serialization;

/// <summary>
/// PCD request base.
/// </summary>
public class PcdRequestCore
{
    /// <summary>
    /// Gets or sets the Pcd request id.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request Type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request DecisionRequiredBy.
    /// </summary>
    [JsonPropertyName("decisionRequiredBy")]
    public string? DecisionRequiredBy { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request DecisionRequested.
    /// </summary>
    [JsonPropertyName("decisionRequested")]
    public string? DecisionRequested { get; set; }
}
