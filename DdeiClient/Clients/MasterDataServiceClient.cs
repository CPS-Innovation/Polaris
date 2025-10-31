// <copyright file="MdsClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using Azure.Storage.Blobs.Models;
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response.HouseKeeping;
    using Cps.Fct.Hk.Ui.Interfaces.Model;
    using DdeiClient.Clients.Interfaces;
    using DdeiClient.Diagnostics;
    using DdeiClient.Model;
    using DdeiClient.Utils;
    using Microsoft;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.Extensions.Logging;

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

        public async Task<Common.Dto.Response.HouseKeeping.UsedStatementsResponse> GetUsedStatementsAsync(GetUsedStatementsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetUsedStatements";

            try
            {
                Common.Dto.Response.HouseKeeping.UsedStatementsResponse? results = new();

                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                string additionalInfo = $"received #0 used statements";
                var data = await client.GetUsedStatementsAsync(request.CaseId);

                if (data?.Statements is not null)
                {
                    results = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse
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

        /// <inheritdoc/>
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

                ICollection<Cps.MasterDataService.Infrastructure.ApiClient.HkCommunicationsInfo>? data = await client.ListCommunicationsHkAsync(request.CaseId);

                if (data is not null)
                {
                    results = data.Select(
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
                         communication.Date?.DateTime)).ToList();
                }

                if (results.Count != 0)
                {
                    additionalInfo = $"received #{results.Count} communications";
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
        public async Task<AttachmentsResponse> GetAttachmentsAsync(GetAttachmentsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetAttachments";
            AttachmentsResponse results = new();

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);
                string additionalInfo = $"received #0 attachments";

                var data = await client.GetAttachmentsAsync(request.CommunicationId);
                if (data?.Attachments is not null)
                {
                    results = new AttachmentsResponse
                    {
                        Attachments = data.Attachments.Select(attachment => new Attachment(
                            attachment.MaterialId,
                            attachment.Name,
                            attachment.Description,
                            attachment.Link,
                            attachment.Classification,
                            attachment.DocumentTypeId,
                            attachment.NumOfDocVersions,
                            MapStatementSubType(attachment.Statement),
                            MapExhibitSubItem(attachment.Exhibit),
                            attachment.Tag,
                            attachment.DocId,
                            attachment.OriginalFileName,
                            attachment.CheckedOutTo,
                            attachment.DocumentId,
                            attachment.OcrProcessed,
                            attachment.Direction)).ToList(),
                    };
                }

                if (results.Attachments.Count != 0)
                {
                    additionalInfo = $"received #{results.Attachments.Count} attachments";
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
        public async Task<FileStreamResult?> GetMaterialDocumentAsync(GetDocumentRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetMaterialDocument";
            FileStreamResult? results;

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                int caseId = int.Parse(request.CaseId);

                var data = await client.GetMaterialDocumentAsync(caseId, request.FilePath);

                string fileDownloadName = Path.GetFileName(request.FilePath);
                string? contentType = FileUtils.GetMimeType(fileDownloadName) ??
                    throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Content type cannot be determined for file: {fileDownloadName}");

                // Create FileStreamResult using the stream and content type
                results = new FileStreamResult(data.Stream, contentType)
                {
                    FileDownloadName = fileDownloadName,
                };

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, fileDownloadName);
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<ExhibitProducersResponse> GetExhibitProducersAsync(GetExhibitProducersRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetExhibitProducers";
            ExhibitProducersResponse results = new();

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                string additionalInfo = "received #0 producers";

                var data = await client.GetExhibitProducersAsync(request.CaseId);

                if (data != null)
                {
                    results = new ExhibitProducersResponse
                    {
                        ExhibitProducers = data.ExhibitProducers?.Select(x => new ExhibitProducer(
                            x.Id,
                            x.Producer)).ToList(),
                    };

                    if (results.ExhibitProducers?.Count > 0)
                    {
                        additionalInfo = $"retrieved #{results.ExhibitProducers.Count} producers";
                    }
                }

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<DefendantsResponse> GetCaseDefendantsAsync(ListCaseDefendantsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "ListCaseDefendants";
            List<Defendant> results = new();

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var data = await client.ListCaseDefendantsAsync(request.CaseId);

                var listCaseDefendantsResponse = new DefendantsResponse
                {
                    Defendants = data.Select(defendant => new Defendant(
                        defendant.Id,
                        defendant.CaseId,
                        defendant.ListOrder,
                        defendant.Type,
                        defendant.FirstNames,
                        defendant.Surname,
                        DateTime.Parse(defendant?.Dob),
                        defendant.PoliceRemandStatus.ToString(),
                        defendant.Youth,
                        defendant?.CustodyTimeLimit.ToString(),
                        MapOffences(defendant.Offences),
                        null,
                        MapProposedCharges(defendant.ProposedCharges),
                        defendant.NextHearing,
                        defendant.DefendantPcdReview,
                        defendant.Solicitor,
                        MapPersonalDetail(defendant.PersonalDetail))).ToList(),
                };

                string additionalInfo = listCaseDefendantsResponse?.Defendants is { Count: > 0 }
                    ? $"received #{listCaseDefendantsResponse.Defendants.Count} defendants"
                    : "received #0 defendants";
                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);

                return listCaseDefendantsResponse;
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<WitnessesResponse> GetCaseWitnessesAsync(GetCaseWitnessesRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "ListCaseWitnesses";
            string additionalInfo = $"received #0 witnesses";
            WitnessesResponse results = new();

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var data = await client.ListCaseWitnessesAsync(request.CaseId);

                if (data is not null)
                {
                    results.Witnesses = data.Select(witness => new Common.Dto.Response.HouseKeeping.Witness(
                        witness.CaseId,
                        witness.WitnessId,
                        witness.FirstName,
                        witness.Surname)).ToList();
                }

                if (results.Witnesses.Count > 0)
                {
                    additionalInfo = $"retrieved #{results.Witnesses.Count} witnesses";
                }

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, additionalInfo);
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<WitnessStatementsResponse> GetWitnessStatementsAsync(GetWitnessStatementsRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "GetStatementsForWitness";
            string additionalInfo = "received #0 statements";

            WitnessStatementsResponse results = new();

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var data = await client.GetStatementsForWitnessAsync(request.WitnessId);

                if (data?.StatementsForWitness is not null)
                {
                    results.WitnessStatements = data.StatementsForWitness.Select(statement =>
                        new WitnessStatement(statement.Id, statement.Title)).ToList();
                }

                if (results.WitnessStatements?.Count > 0)
                {
                    additionalInfo = $"retrieved #{results.WitnessStatements.Count} statements";
                }
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<NoContentResult> AddCaseActionPlanAsync(int caseId, AddActionPlanRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "AddActionPlan";

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var mdsRequest = MapApAction(request);

                await client.AddActionPlanAsync(caseId, mdsRequest);

                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, "");
                return new NoContentResult();
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NoContentResult> AddWitnessAsync(AddWitnessRequest request, CmsAuthValues cmsAuthValues)
        {
            Requires.NotNull(request);
            Requires.NotNull(request.caseId);
            Requires.NotNull(request.Surname);
            Requires.NotNull(cmsAuthValues);
            Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
            Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

            var stopwatch = Stopwatch.StartNew();
            const string OperationName = "AddWitness";

            try
            {
                var cookie = new MasterDataServiceCookie(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
                var cookieString = JsonSerializer.Serialize(cookie);
                var client = this.mdsApiClientFactory.Create(cookieString);

                var mdsRequest = new Cps.MasterDataService.Infrastructure.ApiClient.Witness()
                {
                    CaseId = request.caseId,
                    Surname = request.Surname,
                    FirstName = request.FirstName,
                };

                await client.AddWitnessAsync(request.caseId, mdsRequest);
                this.LogOperationCompletedEvent(OperationName, request, stopwatch.Elapsed, string.Empty);
                return new NoContentResult();
            }
            catch (Exception exception)
            {
                this.HandleException(OperationName, exception, request, stopwatch.Elapsed);
                throw;
            }
        }

        private static ExhibitAttachmentSubType MapExhibitSubItem(Cps.MasterDataService.Infrastructure.ApiClient.ExhibitAttachmentHkSubType exhibit)
        {
            if (exhibit is null)
            {
                return null;
            }

            return new ExhibitAttachmentSubType(exhibit.Reference, exhibit.Item, exhibit.Producer);
        }

        private static StatementAttachmentSubType MapStatementSubType(Cps.MasterDataService.Infrastructure.ApiClient.StatementAttachmentHkSubType statement)
        {
            if (statement == null)
            {
                return null;
            }

            return new StatementAttachmentSubType(
                statement.WitnessName,
                statement?.WitnessTitle,
                statement?.WitnessShoulderNo,
                statement.StatementNo,
                statement.Date,
                statement?.Witness);
        }

        private static List<Offence> MapOffences(ICollection<Cps.MasterDataService.Infrastructure.ApiClient.Offence> offence)
        {
            if (offence is null)
            {
                return null;
            }

            return new List<Offence>(
                offence.Select(offence =>
                    new Offence(
                        offence.Id,
                        offence.ListOrder,
                        offence.Code,
                        offence.Type,
                        offence.Active,
                        offence.Description,
                        offence.FromDate,
                        offence.ToDate,
                        offence.LatestPlea,
                        offence.LatestVerdict,
                        offence.DisposedReason,
                        offence.LastHearingOutcome,
                        offence.CustodyTimeLimit.ToString(),
                        offence.LatestPleaDescription)));
        }

        private static List<ProposedCharge> MapProposedCharges(ICollection<Cps.MasterDataService.Infrastructure.ApiClient.ProposedCharge> proposedCharges)
        {
            if (proposedCharges is null)
            {
                return null;
            }

            return new List<ProposedCharge>(
                proposedCharges.Select(charge =>
                    new ProposedCharge(
                            charge.Id,
                            charge.CaseId,
                            charge.DefendantId,
                            charge.Surname,
                            charge.FirstNames,
                            charge.Code,
                            charge.Description,
                            MapLocation(charge.Location),
                            charge.FromDate,
                            charge.ToDate,
                            charge.ChargeParticulars,
                            charge.AnticipatedPlea.ToString(),
                            charge.AdjudicationCode.ToString())));
        }

        private static PersonalDetail MapPersonalDetail(Cps.MasterDataService.Infrastructure.ApiClient.DefendantPersonalDetail defendantPersonalDetail)
        {
            if (defendantPersonalDetail is null)
            {
                return null;
            }

            return new PersonalDetail(
               MappAddress(defendantPersonalDetail.Address),
               defendantPersonalDetail.Email,
               defendantPersonalDetail.Ethnicity.ToString(),
               defendantPersonalDetail.Gender.ToString(),
               defendantPersonalDetail.Occupation,
               defendantPersonalDetail.HomePhoneNumber,
               defendantPersonalDetail.MobilePhoneNumber,
               defendantPersonalDetail.WorkPhoneNumber,
               defendantPersonalDetail.PreferredCorrespondenceLanguage.ToString(),
               defendantPersonalDetail.Religion.ToString(),
               defendantPersonalDetail.Guardian);
        }

        private static Address MappAddress(Cps.MasterDataService.Infrastructure.ApiClient.PostalAddress address)
        {
            if (address is null)
            {
                return null;
            }

            return new Address(
                address.Postcode,
                address.AddressLine1,
                address.AddressLine2,
                address.AddressLine3,
                address.AddressLine4,
                address.AddressLine5,
                address.AddressLine6,
                address.AddressLine7,
                address.AddressLine8);
        }

        private static Location MapLocation(Cps.MasterDataService.Infrastructure.ApiClient.InternationalAddress address)
        {
            if (address is null)
            {
                return null;
            }

            return new Location(
                address.Country,
                address.Postcode,
                address.AddressLine1,
                address.AddressLine2,
                address.AddressLine3,
                address.AddressLine4,
                address.AddressLine5,
                address.AddressLine6,
                address.AddressLine7,
                address.AddressLine8);
        }

        private static Cps.MasterDataService.Infrastructure.ApiClient.ApAction MapApAction(AddActionPlanRequest addActionPlanRequest)
        {
            return new Cps.MasterDataService.Infrastructure.ApiClient.ApAction()
            {
                FullDefendantName = addActionPlanRequest.fullDefendantName,
                AllDefendants = addActionPlanRequest.allDefendants,
                Date = ConvertToDateTimeOffset(addActionPlanRequest.date),
                DateExpected = ConvertToDateTimeOffset(addActionPlanRequest.dateExpected.Value),
                DateTimeCreated = addActionPlanRequest.dateTimeCreated,
                ActionPointText = addActionPlanRequest.actionPointText,
                Status = addActionPlanRequest.status,
                StatusDescription = addActionPlanRequest.statusDescription,
                ExpectedDateUpdated = addActionPlanRequest.expectedDateUpdated,
                PartyType = addActionPlanRequest.partyType,
                PoliceChangeReason = addActionPlanRequest.policeChangeReason,
                StatusUpdated = addActionPlanRequest.statusUpdated,
                SyncedWithPolice = addActionPlanRequest.syncedWithPolice,
                CpsChangeReason = addActionPlanRequest.cpsChangeReason,
                ChaserTaskDate = ConvertToDateTimeOffset(addActionPlanRequest.chaserTaskDate),
                Steps = MapActionStep(addActionPlanRequest.steps),
            };
        }

        private static ICollection<Cps.MasterDataService.Infrastructure.ApiClient.ActionStep> MapActionStep(Common.Dto.Request.HouseKeeping.Step[] steps)
        {
            return steps.Select(step => new Cps.MasterDataService.Infrastructure.ApiClient.ActionStep
            {
                Code = step.code,
                Description = step.description,
                Text = step.text,
                Hidden = step.hidden,
                HiddenDraft = step.hiddenDraft,
            }).ToList();
        }

        private static DateTimeOffset ConvertToDateTimeOffset(DateOnly? date, TimeOnly? time = null)
        {
            if (date is null)
            {
                return default;
            }

            TimeOnly actualTime = time ?? TimeOnly.MinValue;
            DateTime dateTime = date.Value.ToDateTime(actualTime);
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
            return new DateTimeOffset(dateTime, offset);
        }

        /// <summary>
        /// Handles an exception that occurred while calling the MDS API.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="request">The request with a correspondence ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        private void HandleException(
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
        private void LogOperationCompletedEvent(
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
        private string BuildAdditionalInfo(UnusedMaterialsResponse results)
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
