// <copyright file="MdsClient.cs" company="TheCrownProsecutionService">
// Copyright (c) TheCrownProsecutionService. All rights reserved.
// </copyright>

namespace MasterDataServiceClient
{
    using System.Diagnostics;
    using System.Text.Json;
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response.HouseKeeping;
    using Common.Logging;
    using MasterDataServiceClient.Configuration;
    using MasterDataServiceClient.Diagnostics;
    using MasterDataServiceClient.Model;
    using Microsoft;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    ///  Repesents Mds API client.
    /// </summary>
    public class MdsClient : IMdsClient
    {
        private readonly IMdsApiClientFactory mdsApiClientFactory;
        private readonly ILogger<MdsClient> logger;
        private readonly MdsClientOptions clientOptions;

        public MdsClient(
            IMdsApiClientFactory mdsApiClientFactory,
            ILoggerFactory loggerFactory,
            IOptions<MdsClientOptions> clientOptions)
        {
            this.logger = loggerFactory.CreateLogger<MdsClient>();
            this.mdsApiClientFactory = mdsApiClientFactory;
            this.clientOptions = clientOptions.Value;
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

            // In two minds wheter to keep this or not given than the MDSClient section in appsettings consisting of relative path of endpoints can be removed as it's not used by
            // the Mds client?
            var apiPath = this.clientOptions.RelativePath[operationName];

            CaseSummaryResponse? result = null;

            try
            {
                var cookie = new MdsCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

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
        /// Handles an exception that occurred while calling the MDS API.
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
            logger.LogError(
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

            logger.LogInformation(
                LoggingConstants.HskUiLogPrefix + " " + LogMessage,
                operationName,
                duration,
                path,
                request.CorrespondenceId,
                additionalInfo);
        }
    }
}
