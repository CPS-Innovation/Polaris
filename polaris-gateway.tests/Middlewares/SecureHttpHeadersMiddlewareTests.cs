// <copyright file="SecureHttpHeadersMiddlewareTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Middlewares;

using Microsoft.AspNetCore.Http;
using PolarisGateway.Middleware;
using Xunit;

/// <summary>
/// SecurityHeadersMiddlewareTests.
/// </summary>
public class SecureHttpHeadersMiddlewareTests
{
    /// <summary>
    /// Check for expected headers.
    /// </summary>
    [Fact]
    public void AddStaticHeaders_Adds_All_Expected_Headers()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        SecureHttpHeadersMiddleware.AddStaticHeaders(context);

        // Assert
        var headers = context.Response.Headers;
        Assert.Equal("max-age=31536000; includeSubDomains", headers["Strict-Transport-Security"]);
        Assert.Equal("", headers["Server"]);
        Assert.Equal("nosniff", headers["X-Content-Type-Options"]);
        Assert.Equal("default-src 'self'", headers["Content-Security-Policy"]);
        Assert.Equal("no-store, max-age-0", headers["Cache-Control"]);
        Assert.Equal("no-cache", headers["Pragma"]);
        Assert.Equal("deny", headers["X-Frame-Options"]);
        Assert.Equal("none", headers["X-Permitted-Cross-Domain-Policies"]);
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
