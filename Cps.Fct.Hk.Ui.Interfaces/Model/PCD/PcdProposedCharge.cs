// <copyright file="PcdProposedCharge.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model.PCD;

using System.Text.Json.Serialization;

/// <summary>
/// PCD Proposed charge.
/// </summary>
public class PcdProposedCharge
{
    /// <summary>
    /// Gets or sets the Pcd ProposedCharge charge.
    /// </summary>
    [JsonPropertyName("charge")]
    public string? Charge { get; set; }

    /// <summary>
    /// Gets or sets the Pcd ProposedCharge EarlyDate.
    /// </summary>
    [JsonPropertyName("earlyDate")]
    public string? EarlyDate { get; set; }

    /// <summary>
    /// Gets or sets the Pcd ProposedCharge lateDate.
    /// </summary>
    [JsonPropertyName("lateDate")]
    public string? LateDate { get; set; }

    /// <summary>
    /// Gets or sets the Pcd ProposedCharge location.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the Pcd ProposedCharge category.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }
}
