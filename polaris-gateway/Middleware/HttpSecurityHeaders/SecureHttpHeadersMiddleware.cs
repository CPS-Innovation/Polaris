// <copyright file="SecureHttpHeadersMiddleware.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Middleware.HttpSecurityHeaders;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Middleware that adds security headers to HTTPS responses.
/// </summary>
public sealed class SecureHttpHeadersMiddleware : IFunctionsWorkerMiddleware
{
    private readonly SecureHttpHeadersOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureHttpHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="options">Security headers.</param>
    public SecureHttpHeadersMiddleware(IOptions<SecureHttpHeadersOptions> options)
    {
        this.options = options.Value;
    }

    [ExcludeFromCodeCoverage]
    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync().ConfigureAwait(false);

        await next(context).ConfigureAwait(false);

        if (request is null || request.Url.Scheme != Uri.UriSchemeHttps)
        {
            return;
        }

        var httpContext = context.GetHttpContext();
        if (httpContext is null)
        {
            return;
        }

        AddStaticHeaders(httpContext, this.options);
        AddDynamicCspHeader(httpContext, request.Url.AbsolutePath);
    }

    /// <summary>
    /// AddStaticHeaders.
    /// </summary>
    /// <param name="httpContext">httpContext.</param>
    /// <param name="options">options.</param>
    public static void AddStaticHeaders(HttpContext httpContext, SecureHttpHeadersOptions options)
    {
        var headers = httpContext.Response.Headers;

        headers.StrictTransportSecurity = options.StrictTransportSecurity;
        headers.Server = options.Server;
        headers.XContentTypeOptions = options.XContentTypeOptions;
        headers.ContentSecurityPolicy = options.ContentSecurityPolicy;
        headers.CacheControl = options.CacheControl;
        headers.Pragma = options.Pragma;
        headers.XFrameOptions = options.XFrameOptions;
        headers["X-Permitted-Cross-Domain-Policies"] = options.XPermittedCrossDomainPolicies;
    }

    /// <summary>
    /// AddDynamicCspHeader.
    /// </summary>
    /// <param name="httpContext">httpContext.</param>
    /// <param name="path">path.</param>
    public static void AddDynamicCspHeader(HttpContext httpContext, string path)
    {
        var csp = IsSwaggerRequest(path)
            ? SwaggerCsp
            : DefaultCsp;

        httpContext.Response.Headers.Append(
            HeaderNames.ContentSecurityPolicy,
            csp);
    }

    private const string SwaggerCsp =
       "default-src 'self'; " +
       "script-src 'self' 'unsafe-inline'; " +
       "style-src 'self' 'unsafe-inline'; " +
       "img-src 'self' data:; " +
       "object-src 'none'; " +
       "frame-ancestors 'none';";

    private const string DefaultCsp =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'none';";

    private static bool IsSwaggerRequest(string path) =>
        path.Contains("/swagger", StringComparison.OrdinalIgnoreCase);
}
