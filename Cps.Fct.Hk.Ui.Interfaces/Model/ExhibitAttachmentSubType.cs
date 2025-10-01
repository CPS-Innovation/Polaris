// <copyright file="ExhibitAttachmentSubType.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the sub-type for an exhibit attachment.
/// </summary>
/// <param name="Reference">The reference for the exhibit.</param>
/// <param name="Item">The item associated with the exhibit.</param>
/// <param name="Producer">The producer of the exhibit.</param>
public record ExhibitAttachmentSubType(
    [property: JsonPropertyName("reference")] string? Reference,
    [property: JsonPropertyName("item")] string? Item,
    [property: JsonPropertyName("producer")] string? Producer);
