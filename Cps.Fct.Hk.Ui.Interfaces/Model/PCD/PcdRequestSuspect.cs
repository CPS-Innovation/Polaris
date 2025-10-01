// <copyright file="PcdRequestSuspect.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model.PCD;

using System.Text.Json.Serialization;

/// <summary>
/// PCD Request suspect.
/// </summary>
public class PcdRequestSuspect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PcdRequestSuspect"/> class.
    /// </summary>
    public PcdRequestSuspect()
    {
        this.ProposedCharges = new List<PcdProposedCharge>();
    }

    /// <summary>
    /// Gets or sets the Pcd request suspect surname.
    /// </summary>
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspect firstNames.
    /// </summary>
    [JsonPropertyName("firstNames")]
    public string? FirstNames { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspect dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public string? Dob { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspect bailConditions.
    /// </summary>
    [JsonPropertyName("bailConditions")]
    public string? BailConditions { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspect bailDate.
    /// </summary>
    [JsonPropertyName("bailDate")]
    public string? BailDate { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspect remandStatus.
    /// </summary>
    [JsonPropertyName("remandStatus")]
    public string? RemandStatus { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspect proposedCharges.
    /// </summary>
    [JsonPropertyName("proposedCharges")]
    public List<PcdProposedCharge>? ProposedCharges { get; set; }
}
