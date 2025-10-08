// <copyright file="MdsClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response.HouseKeeping;
    using DdeiClient.Clients.Interfaces;
    using DdeiClient.Diagnostics;
    using DdeiClient.Model;
    using Microsoft;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;

    /// <summary>
    ///  Repesents Mds API client.
    /// </summary>
    public class MasterDataServiceClient(
        IMasterDataServiceApiClientFactory mdsApiClientFactory,
        ILoggerFactory loggerFactory) : IMasterDataServiceClient
    {
        private readonly IMasterDataServiceApiClientFactory mdsApiClientFactory = mdsApiClientFactory;
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
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
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

        /// <inheritdoc/>
        public async Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(GetUnusedMaterialsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();

            const string OperationName = "GetUnusedMaterials";

            try
            {
                UnusedMaterialsResponse results = new();
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                string additionalInfo = $"received #0 unused materials";
                var data = await client.GetUnusedMaterialsAsync(request.CaseId);

                if (data is not null)
                {
                    results = new UnusedMaterialsResponse
                    {
                        Exhibits = data.Exhibits?.Select(exhibit => new Exhibit(
                            exhibit.Id,
                            exhibit.Title,
                            exhibit.OriginalFileName,
                            string.Empty,
                            exhibit.DocumentType,
                            exhibit.Link,
                            exhibit.Status.ToString(),
                            exhibit.ReceivedDate?.DateTime,
                            exhibit.Reference,
                            exhibit.Producer)).ToList(),

                        MgForms = data.MgForms?.Select(mgForm => new MgForm(
                            mgForm.Id,
                            mgForm.Title,
                            mgForm.OriginalFileName,
                            mgForm.MaterialType,
                            null,
                            mgForm.Link,
                            mgForm.Date.ToString(),
                            mgForm.Date?.DateTime)).ToList(),

                        OtherMaterials = data.OtherMaterials?.Select(otherMaterial => new MgForm(
                            otherMaterial.Id,
                            otherMaterial.Title,
                            otherMaterial.OriginalFileName,
                            otherMaterial.MaterialType,
                            null,
                            otherMaterial.Link,
                            otherMaterial.Status.ToString(),
                            otherMaterial.Date?.DateTime)).ToList(),

                        Statements = data.Statements?.Select(statement => new Statement(
                            statement.Id,
                            statement.WitnessId,
                            statement.Title,
                            statement.OriginalFileName,
                            string.Empty,
                            statement.DocumentType,
                            statement.Link,
                            statement.Status.ToString(),
                            statement.Date?.DateTime,
                            statement.StatementTakenDate?.DateTime)).ToList(),
                    };

                    // Generate additional info string based on the collections
                    additionalInfo = this.BuildAdditionalInfo(results);
                }

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
                return results;
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UsedExhibitsResponse> GetUsedExhibitsAsync(GetUsedExhibitsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetUsedExhibits";

            try
            {
                UsedExhibitsResponse results = new() { Exhibits = [] };
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                string additionalInfo = $"received #0 used exhibits";

                var data = await client.GetUsedExhibitsAsync(request.CaseId);

                if (data?.Exhibits is not null)
                {
                    results = new UsedExhibitsResponse
                    {
                        Exhibits = data.Exhibits.Select(exhibit => new Exhibit(
                            exhibit.Id,
                            exhibit.Title,
                            exhibit.OriginalFileName,
                            string.Empty,
                            exhibit.DocumentType,
                            exhibit.Link,
                            exhibit.Status.ToString(),
                            exhibit.ReceivedDate?.DateTime,
                            exhibit.Reference,
                            exhibit.Producer)).ToList(),
                    };

                    if (results.Exhibits.Count != 0)
                    {
                        additionalInfo = $"received #{results.Exhibits.Count} used exhibits";
                    }
                }

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
                return results;
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UsedMgFormsResponse> GetUsedMgFormsAsync(GetUsedMgFormsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetUsedMgForms";

            try
            {
                UsedMgFormsResponse? results = new() { MgForms = [] };
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var data = await client.GetUsedMgFormsAsync(request.CaseId);

                if (data?.MgForms is not null)
                {
                    results = new UsedMgFormsResponse
                    {
                        MgForms = data.MgForms?.Select(mgForm => new MgForm(
                             mgForm.Id,
                             mgForm.Title,
                             mgForm.OriginalFileName,
                             mgForm.MaterialType,
                             null,
                             mgForm.Link,
                             mgForm.Date.ToString(),
                             mgForm.Date?.DateTime)).ToList(),
                    };
                }

                string additionalInfo = results?.MgForms is { Count: > 0 }
              ? $"received #{results.MgForms.Count} used MG forms"
              : "received #0 used MG forms";

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
                return results;
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(GetUsedOtherMaterialsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetUsedOtherMaterials";

            try
            {
                UsedOtherMaterialsResponse? results = new() { MgForms = [] };
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var data = await client.GetUsedOtherMaterialsAsync(request.CaseId);

                if (data?.MgForms is not null)
                {
                    results = new UsedOtherMaterialsResponse
                    {
                        MgForms = data.MgForms?.Select(mgForm => new MgForm(
                             mgForm.Id,
                             mgForm.Title,
                             mgForm.OriginalFileName,
                             mgForm.MaterialType,
                             null,
                             mgForm.Link,
                             mgForm.Date.ToString(),
                             mgForm.Date?.DateTime)).ToList(),
                    };
                }

                string additionalInfo = results?.MgForms is { Count: > 0 }
             ? $"received #{results.MgForms.Count} used other materials"
             : "received #0 used other materials";
                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
                return results;
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        public async Task<UsedStatementsResponse> GetUsedStatementsAsync(GetUsedStatementsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetUsedStatements";

            try
            {
                UsedStatementsResponse? results = new();

                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                string additionalInfo = $"received #0 used statements";
                var data = await client.GetUsedStatementsAsync(request.CaseId);

                if (data?.Statements is not null)
                {
                    results = new UsedStatementsResponse
                    {
                        Statements = data.Statements.Select(statement => new Statement(
                        statement.Id,
                        statement.WitnessId,
                        statement.Title,
                        statement.OriginalFileName,
                        string.Empty,
                        statement.DocumentType,
                        statement.Link,
                        statement.Status.ToString(),
                        statement.Date?.DateTime,
                        statement.StatementTakenDate?.DateTime)).ToList(),
                    };
                }

                if (results.Statements.Count != 0)
                {
                    additionalInfo = $"received #{results.Statements.Count} used statements";
                }

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
                return results;
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        public async Task<IReadOnlyCollection<Communication>> ListCommunicationsHkAsync(ListCommunicationsHkRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "ListCommunicationsHk";

            try
            {
                List<Communication> results = [];
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);
                string additionalInfo = $"received #0 communications";

                var data = await client.ListCommunicationsHkAsync(request.CaseId);
                if (data is not null)
                {
                    results = new List<Communication>()
                    {
                        data.Select(
                            communication => new Communication(
                             communication.Id.GetValueOrDefault(),
                             communication.OriginalFileName,
                             communication.Subject,
                             communication.DocumentId,
                             communication.MaterialId.GetValueOrDefault(),
                             communication.Status,
                             string.Empty,
                             string.Empty,
                             null,
                             communication.HasAttachments,
                             communication.Method,
                             communication.Direction,
                             communication.Party,
                             communication.Date?.DateTime
,                    }
                }


            }
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
                $"{LogMessage}",
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
                LogMessage,
                operationName,
                string.Empty,
                duration,
                request.CorrespondenceId,
                additionalInfo);
        }

        /// <summary>
        /// Builds a string containing information about the unused materials, based on the provided response.
        /// The information includes the number of unused exhibits, mg forms, other materials, and statements.
        /// </summary>
        /// <param name="results">The response object containing the lists of unused materials.</param>
        /// <returns>A string that describes the received unused materials, including counts for each type.</returns>
        public string BuildAdditionalInfo(UnusedMaterialsResponse results)
        {
            string additionalInfo = string.Empty;
            bool addedInfo = false;

            if (results.Exhibits?.Count > 0)
            {
                additionalInfo += $"received #{results.Exhibits.Count} unused exhibits";
                addedInfo = true;
            }

            if (results.MgForms?.Count > 0)
            {
                additionalInfo += (addedInfo ? "," : "") + $" received #{results.MgForms.Count} unused mg forms";
                addedInfo = true;
            }

            if (results.OtherMaterials?.Count > 0)
            {
                additionalInfo += (addedInfo ? "," : "") + $" received #{results.OtherMaterials.Count} unused other materials";
                addedInfo = true;
            }

            if (results.Statements?.Count > 0)
            {
                additionalInfo += (addedInfo ? "," : "") + $" received #{results.Statements.Count} unused statements";
            }

            if (!addedInfo)
            {
                additionalInfo = "received #0 unused materials";
            }

            return additionalInfo;
        }
    }
}
