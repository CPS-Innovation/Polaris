// <copyright file="PcdComments.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model.PCD;

using System.Text.Json.Serialization;

/// <summary>
/// PCD Comments.
/// </summary>
public class PcdComments
{
    /// <summary>
    /// Gets or sets the PCD Comments text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the PCD Comments textWithCmsMarkup.
    /// </summary>
    [JsonPropertyName("textWithCmsMarkup")]
    public string? TextWithCmsMarkup { get; set; }
}
