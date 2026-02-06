// <copyright file="SecureHttpHeadersOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Middleware.HttpSecurityHeaders;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Security Headers Options.
/// </summary>
[ExcludeFromCodeCoverage]
public class SecureHttpHeadersOptions
{
    /// <summary>
    /// DefaultSectionName.
    /// </summary>
    public const string DefaultSectionName = "SecurityHeadersOptions";

    /// <summary>
    /// Gets or sets StrictTransportSecurity.
    /// </summary>
    public string StrictTransportSecurity { get; set; } = "max-age=31536000; includeSubDomains";

    /// <summary>
    /// Gets or sets Server.
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets XContentTypeOptions.
    /// </summary>
    public string XContentTypeOptions { get; set; } = "nosniff";

    /// <summary>
    /// Gets or sets ContentSecurityPolicy.
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'";

    /// <summary>
    /// Gets or sets CacheControl.
    /// </summary>
    public string CacheControl { get; set; } = "no-store, max-age-0";

    /// <summary>
    /// Gets or sets Pragma.
    /// </summary>
    public string Pragma { get; set; } = "no-cache";

    /// <summary>
    /// Gets or sets xFrameOptions.
    /// </summary>
    public string XFrameOptions { get; set; } = "deny";

    /// <summary>
    /// Gets or sets XPermittedCrossDomainPolicies.
    /// </summary>
    public string XPermittedCrossDomainPolicies { get; set; } = "none";
}
