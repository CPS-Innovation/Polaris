// <copyright file="InitService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides the implementation for processing initialization requests.
/// </summary>
public class InitService : IInitService
{
    private readonly ILogger<InitService> logger;
    private readonly IConfiguration config;
    private readonly IDdeiServiceClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging information and errors.</param>
    /// <param name="config">The configuration instance used for retrieving settings.</param>
    /// <param name="apiClient">The API client instance used for communication with external services.</param>
    public InitService(ILogger<InitService> logger, IConfiguration config, IDdeiServiceClient apiClient)
    {
        Requires.NotNull(logger);
        Requires.NotNull(config);
        Requires.NotNull(apiClient);
        this.logger = logger;
        this.config = config;
        this.apiClient = apiClient;
    }

    /// <inheritdoc />
    public async Task<InitResult> ProcessRequest(HttpRequest req, int? caseId, string? cc, string? displayContext)
    {
        // Retrieve values from configuration
        string redirectUrlCwa = this.config["RedirectUrl:CaseworkApp"] ?? string.Empty;
        string redirectUrlHkUi = this.config["RedirectUrl:HousekeepingUi"] ?? string.Empty;

        if (string.IsNullOrEmpty(redirectUrlCwa) || string.IsNullOrEmpty(redirectUrlHkUi))
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} One or more Housekeeping re-direct URL's are missing.");

            return new InitResult
            {
                Status = InitResultStatus.ServerError,
                Message = "One or more Housekeeping re-direct URL's are missing.",
            };
        }

        // Log a warning when the cc query parameter is missing
        if (string.IsNullOrEmpty(cc))
        {
            this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} The 'cc' query parameter is missing from the request for [{caseId}]");
        }

        // Log when the screen query parameter is missing, empty or contains whitespace
        if (string.IsNullOrWhiteSpace(displayContext))
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} The 'displayContext' query parameter is missing from the request for [{caseId}]");
        }

        // Set cookies only if cc is available
        if (caseId.HasValue && !string.IsNullOrEmpty(cc))
        {
            string? ct = null;

            try
            {
                var correspondenceId = Guid.NewGuid();
                var cmsAuthValues = new CmsAuthValues(cc, Guid.NewGuid().ToString(), correspondenceId);
                var request = new GetCmsModernTokenRequest(cmsAuthValues, correspondenceId);
                ct = await this.apiClient.GetCmsModernTokenAsync(request).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Unable to process request for caseId [{caseId}]: ", ex.Message);
            }

            if (string.IsNullOrWhiteSpace(displayContext))
            {
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Redirect URL for caseId [{caseId}] without display context [{redirectUrlHkUi}]");

                return new InitResult
                {
                    Status = InitResultStatus.Redirect,
                    RedirectUrl = redirectUrlHkUi, // HK UI home page
                    ShouldSetCookie = true,
                    CaseId = caseId.Value.ToString(CultureInfo.InvariantCulture),
                    Cc = cc,
                    Ct = ct,
                };
            }
            else
            {
                // Check if the base URL already has query parameters
                string separator = redirectUrlHkUi.Contains('?') ? "&" : "?";

                // Properly encode the displayContext and redirect to the HK UI home page
                string redirectUrlWithDisplayContext = redirectUrlHkUi + separator + "screen=" + Uri.EscapeDataString(displayContext);

                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Redirect URL for caseId [{caseId}] with display context [{redirectUrlWithDisplayContext}]");

                return new InitResult
                {
                    Status = InitResultStatus.Redirect,
                    RedirectUrl = redirectUrlWithDisplayContext, // HK UI home page with displayContext in the path
                    ShouldSetCookie = true,
                    CaseId = caseId.Value.ToString(CultureInfo.InvariantCulture),
                    Cc = cc,
                    Ct = ct,
                };
            }
        }

        // Redirect to CWA if cc is missing
        string redirectUrl;
        if (string.IsNullOrEmpty(displayContext))
        {
            redirectUrl = this.BuildRedirectUrl(req, caseId, redirectUrlCwa);
        }
        else
        {
            redirectUrl = this.BuildRedirectUrl(req, caseId, redirectUrlCwa, displayContext);
        }

        return new InitResult
        {
            Status = InitResultStatus.Redirect,
            RedirectUrl = redirectUrl, // CWA cookie handoff endpoint
        };
    }

    /// <summary>
    /// Builds the full redirect URL for the case initialization process.
    /// </summary>
    /// <param name="req">The HTTP request that contains the scheme and host information.</param>
    /// <param name="caseId">The case ID to be used in the redirect query string.</param>
    /// <param name="redirectUrlCwa">The base redirect URL for the CWA (Casework App).</param>
    /// <param name="displayContext">The optional path representing the display context for the Housekeeping UI.</param>
    /// <returns>A complete redirect URL combining the base URL and the query string with the case ID.</returns>
    internal string BuildRedirectUrl(HttpRequest req, int? caseId, string redirectUrlCwa, string? displayContext = null)
    {
        if (caseId == null)
        {
            throw new ArgumentNullException(nameof(caseId), "Case ID cannot be null.");
        }

        // Get the host from the request
        string host = $"{req.Scheme}://{req.Host}";

        // Build up redirect query string
        string redirectEndpoint = $"/api/init/{caseId.Value.ToString(CultureInfo.InvariantCulture)}";

        // Append query parameter if displayContext is provided
        string queryString = !string.IsNullOrWhiteSpace(displayContext)
            ? $"?screen={Uri.EscapeDataString(displayContext)}"
            : string.Empty;

        string redirectUrl = $"{redirectUrlCwa}{host}{redirectEndpoint}{queryString}";

        this.logger.LogInformation(
            $"{LoggingConstants.HskUiLogPrefix} Building redirect URL for caseId [{caseId}] " +
            $"{(displayContext != null ? $"with display context [{redirectUrl}]" : $"without display context [{redirectUrl}]")}");

        return redirectUrl;
    }
}
