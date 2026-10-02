// <copyright file="RedactionLoggerAuthDelegatingHandler.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.RedactionLogger;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class RedactionLoggerAuthDelegatingHandler(
    IOptions<RedactionLoggerConfig> redactionLoggerConfigOptions,
    IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private readonly string[] scopes =
        [Validate(nameof(RedactionLoggerConfig.Scope), redactionLoggerConfigOptions?.Value?.Scope)];

    private readonly IConfidentialClientApplication confidentialClientApplication =
        ConfidentialClientApplicationBuilder
            .Create(Validate(nameof(RedactionLoggerConfig.ClientId), redactionLoggerConfigOptions?.Value?.ClientId))
            .WithClientSecret(Validate(nameof(RedactionLoggerConfig.ClientSecret), redactionLoggerConfigOptions?.Value?.ClientSecret))
            .WithAuthority($"https://login.microsoftonline.com/{Validate(nameof(RedactionLoggerConfig.TenantId), redactionLoggerConfigOptions?.Value?.TenantId)}")
            .Build();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = this.GetIncomingAccessToken(request);

        var tokenResult = await this.confidentialClientApplication
            .AcquireTokenOnBehalfOf(this.scopes, new UserAssertion(accessToken))
            .ExecuteAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private static string Validate(string propertyName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"RedactionLogger configuration value cannot be null or empty: {propertyName}");
        }

        return value;
    }

    private string GetIncomingAccessToken(HttpRequestMessage request)
    {
        if (request.Headers.Authorization is { Scheme: var scheme, Parameter: var parameter }
            && string.Equals(scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(parameter))
        {
            return parameter;
        }

        var authorizationHeader = this.httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !AuthenticationHeaderValue.TryParse(authorizationHeader, out var authHeader)
            || !string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authHeader.Parameter))
        {
            throw new InvalidOperationException("A bearer token is required for redaction logger on-behalf-of authentication.");
        }

        return authHeader.Parameter;
    }
}
