// <copyright file="SecureHttpHeadersMiddleware.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Middleware
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Middleware;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Middleware that adds security headers to HTTPS responses.
    /// </summary>
    public sealed class SecureHttpHeadersMiddleware : IFunctionsWorkerMiddleware
    {
        private const string StrictTransportSecurity = "max-age=31536000; includeSubDomains";
        private const string Server = "";
        private const string XContentTypeOptions = "nosniff";
        private const string ContentSecurityPolicy = "default-src 'self'";
        private const string CacheControl = "no-store, max-age-0";
        private const string Pragma = "no-cache";
        private const string XFrameOptions = "deny";
        private const string XPermittedCrossDomainPolicies = "none";

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

            AddStaticHeaders(httpContext);
            AddDynamicCspHeader(httpContext, request.Url.AbsolutePath);
        }

        /// <summary>
        /// AddStaticHeaders.
        /// </summary>
        /// <param name="httpContext">httpContext.</param>
        public static void AddStaticHeaders(HttpContext httpContext)
        {
            var headers = httpContext.Response.Headers;

            headers.StrictTransportSecurity = StrictTransportSecurity;
            headers.Server = Server;
            headers.XContentTypeOptions = XContentTypeOptions;
            headers.ContentSecurityPolicy = ContentSecurityPolicy;
            headers.CacheControl = CacheControl;
            headers.Pragma = Pragma;
            headers.XFrameOptions = XFrameOptions;
            headers["X-Permitted-Cross-Domain-Policies"] = XPermittedCrossDomainPolicies;
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
}
