// <copyright file="ReclassifyExhibitRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

/// <summary>
/// Reclassify exhibit request model.
/// </summary>
public record ReclassifyExhibitRequest
{
    /// <summary>
    /// Gets or sets the exhibit item.
    /// </summary>
    public string Item { get; set; }

    /// <summary>
    /// Gets or sets the exhibit reference number.
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the producer.
    /// </summary>
    public string Producer { get; set; }

    /// <summary>
    /// Gets or sets the name of new producer provided by user.
    /// </summary>
    public string NewProducer { get; set; }

    /// <summary>
    /// Gets or sets existing producer or witness Id.
    /// </summary>
    public int? ExistingProducerOrWitnessId { get; set; }
}
