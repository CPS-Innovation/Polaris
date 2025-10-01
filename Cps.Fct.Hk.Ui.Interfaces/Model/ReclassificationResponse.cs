// <copyright file="ReclassificationResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after a reclassification operation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReclassificationResponse"/> class.
/// </remarks>
/// <param name="reclassifyCommunication">The details of the reclassified communication.</param>
public class ReclassificationResponse(ReclassifyCommunication reclassifyCommunication)
{
    /// <summary>
    /// Gets or sets the details of the reclassified communication.
    /// </summary>
    [JsonPropertyName("reclassifyCommunication")]
    public ReclassifyCommunication ReclassifyCommunication { get; set; } = reclassifyCommunication ?? throw new ArgumentNullException(nameof(reclassifyCommunication));
}

/// <summary>
/// Represents the details of the reclassified communication.
/// </summary>
public class ReclassifyCommunication
{
    /// <summary>
    /// Gets or sets the unique identifier for the reclassified communication.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
