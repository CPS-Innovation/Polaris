// <copyright file="EditExhibitRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Text.Json.Serialization;

/// <summary>
/// Represents update exhibit request object.
/// </summary>
public record EditExhibitRequest(
    [property: JsonPropertyName("materialId")] int MaterialId,
    [property: JsonPropertyName("documentType")] int DocumentType,
    [property: JsonPropertyName("item")] string Item,
    [property: JsonPropertyName("reference")] string Reference,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("used")] bool Used,
    [property: JsonPropertyName("newProducer")] string? NewProducer,
    [property: JsonPropertyName("existingProducerOrWitnessId")] int? ExistingProducerOrWitnessId);
