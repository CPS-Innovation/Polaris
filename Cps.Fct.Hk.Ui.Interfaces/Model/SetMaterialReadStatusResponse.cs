// <copyright file="SetMaterialReadStatusResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after a set material read/unread status.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SetMaterialReadStatusResponse"/> class.
/// </remarks>
/// <param name="completeCommunicationData">The details of the reclassified communication.</param>
public class SetMaterialReadStatusResponse(SetMaterialReadStatusResponseData completeCommunicationData)
{
    /// <summary>
    /// Gets or sets the details of the reclassified communication.
    /// </summary>
    [JsonPropertyName("completeCommunication")]
    public SetMaterialReadStatusResponseData CompleteCommunicationData { get; set; } = completeCommunicationData ?? throw new ArgumentNullException(nameof(completeCommunicationData));
}

/// <summary>
/// Represents the details of the rename material response data..
/// </summary>
public class SetMaterialReadStatusResponseData
{
    /// <summary>
    /// Gets or sets the material id of the renamed material.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
