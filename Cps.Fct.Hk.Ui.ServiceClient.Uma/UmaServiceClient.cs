// <copyright file="UmaServiceClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Uma;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.ServiceClient.Uma.Configuration;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Implementation of the UMA service client interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UmaServiceClient"/> class.
/// </remarks>
/// <param name="logger">The logger instance used for logging information and errors.</param>
/// <param name="umaClientOptions">The options containing UMA client configuration values.</param>
/// <param name="httpClient">The HTTP client used to call the API.</param>
public class UmaServiceClient(ILogger<UmaServiceClient> logger, IOptions<UmaClientOptions> umaClientOptions, HttpClient httpClient)
    : IUmaServiceClient
{
    private readonly ILogger<UmaServiceClient> logger = logger;
    private readonly UmaClientOptions umaClientOptions = umaClientOptions.Value ?? throw new ArgumentNullException(nameof(umaClientOptions));
    private readonly HttpClient httpClient = httpClient;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<MatchedCommunication>> MatchCommunicationsUmAsync(int caseId, IReadOnlyCollection<Communication> communications)
    {
        var stopwatch = Stopwatch.StartNew();
        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Calling the UMA ClassificationService API for MatchCommunicationsUm ...");

        try
        {
            if (string.IsNullOrWhiteSpace(this.umaClientOptions.BaseAddress))
            {
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The UMAClient:BaseAddress configuration is missing or empty.");
            }

            if (string.IsNullOrWhiteSpace(this.umaClientOptions.FunctionKey))
            {
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The UMAClient:FunctionKey configuration is missing or empty.");
            }

            // Construct the request URI
            string requestUri = $"{this.umaClientOptions.BaseAddress}api/cases/{caseId}/match-communications-um";

            // Create the HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(communications),
            };

            this.SetFunctionKeyHeader(request);

            // Send the POST request with the communications collection as the body
            HttpResponseMessage response = await this.httpClient
                .PostAsJsonAsync(requestUri, communications)
                .ConfigureAwait(false);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Parse the response body into the expected collection
            if (response.Content.Headers.ContentLength == 0)
            {
                return new List<MatchedCommunication>();
            }

            List<MatchedCommunication>? matchedCommunications = await response.Content
            .ReadFromJsonAsync<List<MatchedCommunication>>()
            .ConfigureAwait(false);

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Calling the UMA ClassificationService API succeeded for MatchCommunicationsUm in [{stopwatch.Elapsed}]");

            // Return the result or an empty collection if the response was null
            return matchedCommunications ?? new List<MatchedCommunication>();
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Calling the UMA ClassificationService API failed for MatchCommunicationsUm: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Sets the function key header on the specified HTTP request.
    /// </summary>
    /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to which the function key header will be added.</param>
    /// <remarks>
    /// This method adds the `x-functions-key` header to the provided HTTP request using the function key stored in the client options.
    /// If the function key is not null or whitespace and is longer than seven characters, a sanitised version of the key (showing only the last seven characters) is logged for security purposes.
    /// </remarks>
    private void SetFunctionKeyHeader(HttpRequestMessage httpRequest)
    {
        const int Seven = 7;
        string sanitisedFunctionKey = "Unknown";
        if (!string.IsNullOrWhiteSpace(this.umaClientOptions.FunctionKey))
        {
            httpRequest.Headers.Add("x-functions-key", this.umaClientOptions.FunctionKey);
            if (this.umaClientOptions.FunctionKey.Length > Seven)
            {
                // Last 7 characters
                sanitisedFunctionKey = $"...{this.umaClientOptions.FunctionKey[^Seven..]}";
            }
        }

        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Calling the UMA ClassificationService with sanitised function key '{sanitisedFunctionKey}'");
    }
}
