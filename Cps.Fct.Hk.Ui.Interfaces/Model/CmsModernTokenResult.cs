// <copyright file="CmsModernTokenResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the result containing the CMS modern token.
/// </summary>
public record CmsModernTokenResult(
    [property: JsonPropertyName("cmsModernToken")] string CmsModernToken)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CmsModernTokenResult"/> class.
    /// </summary>
    public CmsModernTokenResult()
        : this(string.Empty)
    {
    }
}
