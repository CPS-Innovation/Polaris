// <copyright file="MdsApiClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MdsClient
{
    using System.Diagnostics;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response.HouseKeeping;
    using Common.Logging;
    using Cps.MasterDataService.Infrastructure.ApiClient;
    using MdsClient.Configuration;
    using MdsClient.Diagnostics;
    using MdsClient.Model;
    using Microsoft;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Mds api client factory.
    /// </summary>
    public class MdsApiClientFactory : IMdsApiClientFactory
    {
        private readonly ILogger<MdsApiClientFactory> logger;
        private readonly MdsClientOptions clientOptions;
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MdsApiClientFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="clientOptions">The client endpoint options.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public MdsApiClientFactory(
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IOptions<MdsClientOptions> clientOptions)
        {
            Requires.NotNull(nameof(loggerFactory));
            Requires.NotNull(nameof(clientOptions));
            Requires.NotNull(nameof(httpClientFactory));

            this.logger = loggerFactory.CreateLogger<MdsApiClientFactory>();
            this.clientOptions = clientOptions.Value;
            this.httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        public IMdsApiClient Create(string cookieHeader)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cookieHeader))
                {
                    this.logger.LogError("Cookie header is required.");
                    throw new ArgumentNullException(nameof(cookieHeader), "Cookie header is required.");
                }

                var functionKey = this.clientOptions.FunctionKey;
                if (string.IsNullOrWhiteSpace(functionKey))
                {
                    this.logger.LogError("Missing MDS function key in configuration.");
                    throw new InvalidOperationException("Missing MDS function key in configuration.");
                }

                var baseUrl = this.clientOptions.BaseAddress.ToString();
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    this.logger.LogError("Missing MDS base url in configuration.");
                    throw new InvalidOperationException("Missing MDS base url in configuration.");
                }

                // Create the HttpClient using a named client
                var client = this.httpClientFactory.CreateClient("MdsClient");

                client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", cookieHeader);

                return new MdsApiClient(client) { BaseUrl = baseUrl };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while creating Mds api client.");
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CaseSummaryResponse?> GetCaseSummaryAsync(GetCaseSummaryRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();

            var operationName = "GetCaseSummary";

            // In two minds wheter to keep this or not given than the DDEIClient section in appsettings consisting of relative path of endpoints can be removed as it's not used by
            // the Mds client?
            var apiPath = this.clientOptions.RelativePath[operationName];

            CaseSummaryResponse? result = null;

            try
            {
                var cookie = new MdsCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.Create(cookieString);

                var data = await client.GetCaseSummaryAsync(request.CaseId);

                if (data?.Urn is not null)
                {
                    result = new CaseSummaryResponse(
                        data.Id,
                        data.Urn,
                        data.LeadDefendantFirstNames,
                        data.LeadDefendantSurname,
                        data.NumberOfDefendants,
                        data.UnitName);
                }

                this.LogOperationCompletedEvent(operationName, apiPath, request, stopwatch.Elapsed, string.Empty);
            }
            catch (Exception exception)
            {
               this.HandleException(operationName, apiPath, exception, request, stopwatch.Elapsed);
               throw;
            }

            return result;
        }

        /// <summary>
        /// Handles an exception that occurred while calling the DDEI API.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="path">The relative path of the API call.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="request">The request with a correspondence ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        public void HandleException(
            string operationName,
            string path,
            Exception exception,
            BaseRequest request,
            TimeSpan duration)
        {
            Requires.NotNull(operationName);
            Requires.NotNull(path);
            Requires.NotNull(exception);

            const string LogMessage = DiagnosticsUtility.Error + @"Calling the MDS API failed for {Operation} after {Duration}. Path: {Path}, Correspondence ID: {CorrespondenceId}, Failure: {Reason}
 - Failure response: {FailureResponse}";
            this.logger.LogError(
                exception,
                LoggingConstants.HskUiLogPrefix + " " + LogMessage,
                operationName,
                duration,
                path,
                request?.CorrespondenceId,
                exception.ToAggregatedMessage(),
                string.Empty);
        }

        /// <summary>
        /// Logs an operation completed event.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="path">The relative path of the API call.</param>
        /// <param name="request">The request with a correspondence ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="additionalInfo">Any additional information.</param>
        public void LogOperationCompletedEvent(
            string operationName,
            string path,
            BaseRequest request,
            TimeSpan duration,
            string additionalInfo)
        {
            const string LogMessage = @"Calling the MDS API succeeded for {Operation} after {Duration}. Path: {Path}, Correspondence ID: {CorrespondenceId}
 - Additional info: {AdditionalInfo}";

            this.logger.LogInformation(
                LoggingConstants.HskUiLogPrefix + " " + LogMessage,
                operationName,
                duration,
                path,
                request.CorrespondenceId,
                additionalInfo);
        }

    }
}
