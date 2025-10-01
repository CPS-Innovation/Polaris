// <copyright file="PcdCaseOutlineLine.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model.PCD;

using System.Text.Json.Serialization;

/// <summary>
/// PCD Outline data.
/// </summary>
public class PcdCaseOutlineLine
{
    /// <summary>
    /// Gets or sets the PCD request outline Heading.
    /// </summary>
    [JsonPropertyName("heading")]
    public string? Heading { get; set; }

    /// <summary>
    /// Gets or sets the PCD request outline Text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the PCD request outline TextWithCmsMarkup.
    /// </summary>
    [JsonPropertyName("textWithCmsMarkup")]
    public string? TextWithCmsMarkup { get; set; }
}
