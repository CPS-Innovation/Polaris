// <copyright file="SecureHttpHeadersMiddlewareTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Middlewares;

using Microsoft.AspNetCore.Http;
using PolarisGateway.Middleware.HttpSecurityHeaders;
using Xunit;

/// <summary>
/// SecurityHeadersMiddlewareTests.
/// </summary>
public class SecureHttpHeadersMiddlewareTests
{
    private readonly SecureHttpHeadersOptions optionsMock = new()
    {
        StrictTransportSecurity = "max-age=31536000",
        Server = "CustomServer",
        XContentTypeOptions = "nosniff",
        ContentSecurityPolicy = "default-src 'self'",
        CacheControl = "no-cache",
        Pragma = "no-cache",
        XFrameOptions = "DENY",
        XPermittedCrossDomainPolicies = "none",
    };

    /// <summary>
    /// Check for expected headers.
    /// </summary>
    [Fact]
    public void AddStaticHeaders_Adds_All_Expected_Headers()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        SecureHttpHeadersMiddleware.AddStaticHeaders(context, this.optionsMock);

        // Assert
        var headers = context.Response.Headers;
        Assert.Equal(this.optionsMock.StrictTransportSecurity, headers["Strict-Transport-Security"]);
        Assert.Equal(this.optionsMock.Server, headers["Server"]);
        Assert.Equal(this.optionsMock.XContentTypeOptions, headers["X-Content-Type-Options"]);
        Assert.Equal(this.optionsMock.ContentSecurityPolicy, headers["Content-Security-Policy"]);
        Assert.Equal(this.optionsMock.CacheControl, headers["Cache-Control"]);
        Assert.Equal(this.optionsMock.Pragma, headers["Pragma"]);
        Assert.Equal(this.optionsMock.XFrameOptions, headers["X-Frame-Options"]);
        Assert.Equal(this.optionsMock.XPermittedCrossDomainPolicies, headers["X-Permitted-Cross-Domain-Policies"]);
    }

    /// <summary>
    /// Check for swagger headers.
    /// </summary>
    [Fact]
    public void AddDynamicCspHeader_Adds_Inline_CSP_For_Swagger()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        SecureHttpHeadersMiddleware.AddDynamicCspHeader(context, "/swagger/index.html");

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"];
        Assert.Contains("unsafe-inline", csp.ToString());
    }

    /// <summary>
    /// Check for strict csp headers.
    /// </summary>
    [Fact]
    public void AddDynamicCspHeader_Adds_Strict_CSP_For_Normal_Paths()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        SecureHttpHeadersMiddleware.AddDynamicCspHeader(context, "/home");

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"];
        Assert.NotEqual("unsafe-inline", csp.ToString());
        Assert.Contains("default-src 'self'", csp.ToString());
    }
}
