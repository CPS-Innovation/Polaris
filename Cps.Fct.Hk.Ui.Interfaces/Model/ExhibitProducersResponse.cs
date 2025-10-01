// <copyright file="ExhibitProducersResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Exhibit producers response model.
/// </summary>
public record ExhibitProducersResponse
{
    /// <summary>
    /// Gets or sets exhibit producers for a case.
    /// </summary>
    [JsonPropertyName("exhibitProducers")]
    public List<ExhibitProducer>? ExhibitProducers { get; set; }
}

/// <summary>
/// Details of an exhibit producer.
/// </summary>
/// <param name="Id">The unique ID of the exhibit producer.</param>
/// <param name="Name">The producer's name.</param>
public record ExhibitProducer(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("producer")] string Name);
