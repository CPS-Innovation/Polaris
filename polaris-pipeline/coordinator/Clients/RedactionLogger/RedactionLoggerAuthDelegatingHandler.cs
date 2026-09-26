// <copyright file="RedactionLoggerAuthDelegatingHandler.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.RedactionLogger;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class RedactionLoggerAuthDelegatingHandler(IOptions<RedactionLoggerConfig> redactionLoggerConfigOptions)
    : DelegatingHandler
{
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
        var tokenResult = await this.confidentialClientApplication
            .AcquireTokenForClient(this.scopes)
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
}
