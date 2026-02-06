// <copyright file="SecureHttpHeadersOptionsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using PolarisGateway.Middleware.HttpSecurityHeaders;
using Xunit;

namespace PolarisGateway.Tests.Middlewares;

/// <summary>
/// SecurityHeadersOptionsTests unit tests.
/// </summary>
public class SecureHttpHeadersOptionsTests
{
    /// <summary>
    /// HasExpectedValue.
    /// </summary>
    [Fact]
    public void DefaultSectionName_HasExpectedValue()
    {
        // Assert
        Assert.Equal("SecurityHeadersOptions", SecureHttpHeadersOptions.DefaultSectionName);
    }

    /// <summary>
    /// Has expected default values.
    /// </summary>
    [Fact]
    public void Defaults_AreAsExpected()
    {
        // Arrange
        var options = new SecureHttpHeadersOptions();

        // Assert
        Assert.Equal("max-age=31536000; includeSubDomains", options.StrictTransportSecurity);
        Assert.Equal(string.Empty, options.Server);
        Assert.Equal("nosniff", options.XContentTypeOptions);
        Assert.Equal("default-src 'self'", options.ContentSecurityPolicy);
        Assert.Equal("no-store, max-age-0", options.CacheControl);
        Assert.Equal("no-cache", options.Pragma);
        Assert.Equal("deny", options.XFrameOptions);
        Assert.Equal("none", options.XPermittedCrossDomainPolicies);
    }

    /// <summary>
    /// Headers can be updated.
    /// </summary>
    [Fact]
    public void Properties_CanBeUpdated()
    {
        // Arrange
        var options = new SecureHttpHeadersOptions
        {
            StrictTransportSecurity = "strict",
            Server = "CustomServer",
            XContentTypeOptions = "custom",
            ContentSecurityPolicy = "csp",
            CacheControl = "cache",
            Pragma = "pragma",
            XFrameOptions = "frame",
            XPermittedCrossDomainPolicies = "cross",
        };

        // Assert
        Assert.Equal("strict", options.StrictTransportSecurity);
        Assert.Equal("CustomServer", options.Server);
        Assert.Equal("custom", options.XContentTypeOptions);
        Assert.Equal("csp", options.ContentSecurityPolicy);
        Assert.Equal("cache", options.CacheControl);
        Assert.Equal("pragma", options.Pragma);
        Assert.Equal("frame", options.XFrameOptions);
        Assert.Equal("cross", options.XPermittedCrossDomainPolicies);
    }
}
