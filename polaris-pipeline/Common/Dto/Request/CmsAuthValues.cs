// <copyright file="CmsAuthValues.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using System;

namespace Common.Dto.Request;

/// <summary>
/// Represents the authentication values required for CMS.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CmsAuthValues"/> class.
/// </remarks>
/// <param name="cmsAuthFullValue">The CMS authentication string value</param>
/// <param name="correlationId">The correlation ID to track requests.</param>
public class CmsAuthValues(string cmsAuthFullValue, Guid correlationId = default)
{
    /// <summary>
    /// Gets or sets the correlation ID used to track requests.
    /// </summary>
    public Guid CorrelationId { get; set; } = correlationId;

    /// <summary>
    /// Gets or sets the full CMS authentication value.
    /// </summary>
    public string CmsAuthFullValue { get; set; } = cmsAuthFullValue;
}
