// <copyright file="JwksFetcher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System.Threading.Tasks;
using System;
using System.Net.Http;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Provides functionality to fetch JSON Web Key Sets (JWKS) from a specified endpoint.
/// </summary>
public class JwksFetcher(ILogger<JwksFetcher> logger, HttpClient httpClient)
    : IJwksFetcher
{
    private readonly ILogger<JwksFetcher> logger = logger;
    private readonly HttpClient httpClient = httpClient;

    /// <summary>
    /// Fetches the JSON Web Key Set (JWKS) for the specified tenant ID.
    /// </summary>
    /// <param name="tenantId">The tenant ID for which the JWKS is to be fetched.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the JWKS JSON as a string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the `tenantId` is null or empty.
    /// </exception>
    /// <exception cref="HttpRequestException">Thrown if there is an error during the HTTP request.</exception>
    public async Task<string> GetJwksAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The tenantId is null or empty.");
        }

        string jwksUrl = $"https://login.microsoftonline.com/{tenantId}/discovery/keys";

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching JWKS from URL: {jwksUrl}");

            // Start fetching JWKS
            HttpResponseMessage response = await this.httpClient.GetAsync(jwksUrl).ConfigureAwait(true);
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Received HTTP response. Status code: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            string jwks = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Successfully fetched JWKS from URL: {jwksUrl}");

            return jwks;
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} An error occurred while fetching JWKS from URL: {jwksUrl}");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} An unexpected error occurred while fetching JWKS from URL: {jwksUrl}");
            throw;
        }
    }
}
