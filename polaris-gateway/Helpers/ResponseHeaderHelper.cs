// <copyright file="ResponseHeaderHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PolarisGateway.Helpers;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides utility methods for setting caching headers on HTTP responses.
/// </summary>
public static class ResponseHeaderHelper
{
    /// <summary>
    /// Sets headers to prevent caching on the HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response object.</param>
    public static void SetNoCacheHeaders(HttpResponse response)
    {
        response.Headers["Cache-Control"] = "no-store, no-cache";
        response.Headers["Pragma"] = "no-cache";
    }

    /// <summary>
    /// Sets security-related headers on the HTTP response to protect against various attacks.
    /// </summary>
    /// <param name="response">The HTTP response object.</param>
    /// <remarks>Applies headers for content security, transport security, referrer policy, permissions policy, and MIME type sniffing.</remarks>
    public static void SetSecurityHeaders(HttpResponse response)
    {
        response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none';";
        response.Headers["X-Content-Type-Options"] = "nosniff";
        response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        response.Headers["Referrer-Policy"] = "no-referrer";
        response.Headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=(), payment=()";
    }

    /// <summary>
    /// Sets cache control headers to allow caching for a specified duration.
    /// </summary>
    /// <param name="response">The HTTP response object.</param>
    /// <param name="durationInSeconds">The caching duration in seconds.</param>
    public static void SetCacheHeaders(HttpResponse response, int durationInSeconds = 900)
    {
        response.Headers["Cache-Control"] = $"public, max-age={durationInSeconds}";
    }
}
