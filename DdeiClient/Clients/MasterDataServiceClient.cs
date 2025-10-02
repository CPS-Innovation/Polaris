// <copyright file="MdsClient.cs" company="TheCrownProsecutionService">
// Copyright (c) TheCrownProsecutionService. All rights reserved.
// </copyright>

namespace DdeiClient.Clients
{
    using System.Diagnostics;
    using System.Text.Json;
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response.HouseKeeping;
    using Common.Logging;
    using DdeiClient.Clients.Interfaces;
    using DdeiClient.Diagnostics;
    using DdeiClient.Model;
    using Microsoft;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///  Repesents Mds API client.
    /// </summary>
    public class MasterDataServiceClient(
        IMdsApiClientFactory mdsApiClientFactory,
        ILoggerFactory loggerFactory) : IMasterDataServiceClient
    {
        private readonly IMdsApiClientFactory mdsApiClientFactory = mdsApiClientFactory;
        private readonly ILogger<MasterDataServiceClient> logger = loggerFactory.CreateLogger<MasterDataServiceClient>();

        /// <inheritdoc/>
        public async Task<CaseSummaryResponse> GetCaseSummaryAsync(GetCaseSummaryRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();

            var operationName = "GetCaseSummary";

            CaseSummaryResponse result = null;

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

                this.LogOperationCompletedEvent(operationName, request, stopwatch.Elapsed, string.Empty);
            }
            catch (Exception exception)
            {
                this.HandleException(operationName, exception, request, stopwatch.Elapsed);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Handles an exception that occurred while calling the MDS API.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="request">The request with a correspondence ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        public void HandleException(
            string operationName,
            Exception exception,
            BaseRequest request,
            TimeSpan duration)
        {
            Requires.NotNull(operationName);
            Requires.NotNull(exception);

            const string LogMessage = DiagnosticsUtility.Error + @"Calling the MDS API failed for {Operation} after {Duration}. Path: {Path}, Correspondence ID: {CorrespondenceId}, Failure: {Reason}
 - Failure response: {FailureResponse}";
            this.logger.LogError(
                exception,
                $"{LoggingConstants.HskUiLogPrefix} {LogMessage}",
                string.Empty,
                operationName,
                duration,
                request?.CorrespondenceId,
                exception.ToAggregatedMessage(),
                string.Empty);
        }

        /// <summary>
        /// Logs an operation completed event.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="request">The request with a correspondence ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="additionalInfo">Any additional information.</param>
        public void LogOperationCompletedEvent(
            string operationName,
            BaseRequest request,
            TimeSpan duration,
            string additionalInfo)
        {
            const string LogMessage = @"Calling the MDS API succeeded for {Operation} after {Duration}. Path: {Path}, Correspondence ID: {CorrespondenceId}
 - Additional info: {AdditionalInfo}";

            this.logger.LogInformation(
                LoggingConstants.HskUiLogPrefix + " " + LogMessage,
                operationName,
                string.Empty,
                duration,
                request.CorrespondenceId,
                additionalInfo);
        }
    }
}
