// <copyright file="CmsAuthValues.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents the authentication values required for CMS.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CmsAuthValues"/> class.
/// </remarks>
/// <param name="cmsCookies">The CMS cookies, which may be null.</param>
/// <param name="cmsModernToken">The CMS modern API token, which may be null.</param>
/// <param name="correlationId">The correlation ID to track requests.</param>
public class CmsAuthValues(string cmsCookies, string cmsModernToken, Guid correlationId = default)
{
    /// <summary>
    /// Gets or sets the CMS cookies.
    /// </summary>
    public string CmsCookies { get; set; } = cmsCookies;

    /// <summary>
    /// Gets or sets the CMS modern API token.
    /// </summary>
    public string CmsModernToken { get; set; } = cmsModernToken;

    /// <summary>
    /// Gets or sets the correlation ID used to track requests.
    /// </summary>
    public Guid CorrelationId { get; set; } = correlationId;
}
