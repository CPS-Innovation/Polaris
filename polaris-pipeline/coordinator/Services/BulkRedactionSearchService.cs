// <copyright file="BulkRedactionSearchService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Services;

using Castle.Core.Logging;
using Common.Configuration;
using Common.Domain.Document;
using Common.Domain.Ocr;
using Common.Dto.Request;
using Common.Dto.Response.Document;
using Common.Extensions;
using Common.Services.BlobStorage;
using coordinator.Builders;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.Durable.Providers;
using coordinator.Enums;
using coordinator.Search;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BulkRedactionSearchService : IBulkRedactionSearchService
{
    private readonly IOrchestrationProvider orchestrationProvider;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly IBulkRedactionSearchResponseBuilder bulkRedactionSearchResponseBuilder;
    private readonly IOcrDocumentSearch ocrDocumentSearch;
    private readonly IMdsClient mdsClient;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly ILogger<BulkRedactionSearchService> logger;

    public BulkRedactionSearchService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOrchestrationProvider orchestrationProvider, IBulkRedactionSearchResponseBuilder bulkRedactionSearchResponseBuilder, IOcrDocumentSearch ocrDocumentSearch, IConfiguration configuration, IMdsClient mdsClient, IMdsArgFactory mdsArgFactory, ILogger<BulkRedactionSearchService> logger)
    {
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        this.orchestrationProvider = orchestrationProvider.ExceptionIfNull();
        this.bulkRedactionSearchResponseBuilder = bulkRedactionSearchResponseBuilder.ExceptionIfNull();
        this.ocrDocumentSearch = ocrDocumentSearch.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.logger = logger.ExceptionIfNull();
    }

    public async Task<BulkRedactionSearchResponse> InitiateOrOrchestrateOcr(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken)
    {
        var (cmsDocumentDto, failureResponse) = await this.GetDocumentAsync(bulkRedactionSearchDto);

        if (failureResponse is not null)
        {
            return failureResponse;
        }

        var documentPayload = this.CreateDocumentPayload(bulkRedactionSearchDto,cmsDocumentDto);

        await this.SetDocumentStateAsync(cmsDocumentDto, bulkRedactionSearchDto.CaseId);

        var (orchestrationProviderStatus, instanceId) = await this.orchestrationProvider.BulkSearchDocumentAsync(orchestrationClient, documentPayload, cancellationToken);

        this.logger.LogInformation("Bulk Redaction Search, orchestration instance ID {InstanceId}: ", instanceId);

        switch (orchestrationProviderStatus)
        {
            case OrchestrationProviderStatus.Initiated:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshInitiated()
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatus.Processing:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshProcessing()
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatus.Failed:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Orchestration failure")
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatus.Completed:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshCompleted()
                    .Build(bulkRedactionSearchDto);
            default:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Unknown orchestration status")
                    .Build(bulkRedactionSearchDto);
        }
    }

    public async Task<BulkRedactionSearchResponse> GetOcrSearchResults(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken)
    {
        var (cmsDocumentDto, failureResponse) = await this.GetDocumentAsync(bulkRedactionSearchDto);

        if (failureResponse is not null)
        {
            return failureResponse;
        }

        var documentPayload = this.CreateDocumentPayload(
            bulkRedactionSearchDto,
            cmsDocumentDto);

        var orchestrationStatus = await this.orchestrationProvider.GetOrchestrationProviderStatus(orchestrationClient, documentPayload, cancellationToken);

        switch (orchestrationStatus)
        {
            case OrchestrationProviderStatus.Processing:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshProcessing()
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatus.Failed:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Orchestration failure")
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatus.NotStarted:
                return this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Orchestration instance Id invalid", true)
                    .Build(bulkRedactionSearchDto);
        }

        var blobId = new BlobIdType(bulkRedactionSearchDto.CaseId, bulkRedactionSearchDto.MaterialId, bulkRedactionSearchDto.DocumentId, BlobType.Ocr);
        var results = await this.polarisBlobStorageService.TryGetObjectAsync<AnalyzeResults>(blobId);
        if (results is null)
        {
            return this.bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("OCR Document Not Found", true)
                .Build(bulkRedactionSearchDto);
        }

        var ocrDocumentSearchResponse = this.ocrDocumentSearch.Search(bulkRedactionSearchDto.SearchText, results);

        if (!string.IsNullOrEmpty(ocrDocumentSearchResponse.FailureReason))
        {
            return this.bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed(ocrDocumentSearchResponse.FailureReason)
                .Build(bulkRedactionSearchDto);
        }

        return this.bulkRedactionSearchResponseBuilder
            .BuildDocumentRefreshCompleted()
            .BuildRedactionDefinitions(ocrDocumentSearchResponse.RedactionDefinitionDtos)
            .Build(bulkRedactionSearchDto);
    }

    private async Task<(CmsDocumentDto CmsDocumentDto, BulkRedactionSearchResponse FailureResponse)> GetDocumentAsync(BulkRedactionSearchDto bulkRedactionSearchDto)
    {
        var documentType = DocumentNature.GetDocumentNatureType(bulkRedactionSearchDto.MaterialId);

        if (documentType != DocumentNature.Types.Document)
        {
            return (
                null,
                this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Document is not redactable")
                    .Build(bulkRedactionSearchDto));
        }

        var caseIdentifiersArg = this.mdsArgFactory.CreateCaseIdentifiersArg(
            bulkRedactionSearchDto.CmsAuthValues,
            bulkRedactionSearchDto.CorrelationId,
            bulkRedactionSearchDto.Urn,
            bulkRedactionSearchDto.CaseId);

        var listDocumentResponse = await this.mdsClient.ListDocumentsAsync(caseIdentifiersArg);

        var cmsDocumentDto = listDocumentResponse.FirstOrDefault(
            x => bulkRedactionSearchDto.MaterialId.Contains(x.DocumentId.ToString()) &&
                 x.VersionId == bulkRedactionSearchDto.DocumentId);

        if (cmsDocumentDto is null)
        {
            return (
                null,
                this.bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Document not found in list document", true)
                    .Build(bulkRedactionSearchDto));
        }

        return (cmsDocumentDto, null);
    }

    private DocumentPayload CreateDocumentPayload(BulkRedactionSearchDto bulkRedactionSearchDto, CmsDocumentDto cmsDocumentDto)
    {
        return new DocumentPayload
        {
            Urn = bulkRedactionSearchDto.Urn,
            CaseId = bulkRedactionSearchDto.CaseId,
            CmsAuthValues = bulkRedactionSearchDto.CmsAuthValues,
            CorrelationId = bulkRedactionSearchDto.CorrelationId,
            MaterialId = bulkRedactionSearchDto.MaterialId,
            DocumentId = bulkRedactionSearchDto.DocumentId,
            Path = cmsDocumentDto.Path,
            DocumentType = cmsDocumentDto.CmsDocType,
            DocumentNatureType = DocumentNature.Types.Document,
            DocumentDeltaType = DocumentDeltaType.RequiresIndexing,
            IsOcredProcessedPreference = cmsDocumentDto.IsOcrProcessed,
        };
    }

    private async Task SetDocumentStateAsync(CmsDocumentDto cmsDocumentDto, int caseId)
    {
        var documentsStateBlobId = new BlobIdType(caseId, default, default, BlobType.DocumentState);
        var documentState = await this.polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(documentsStateBlobId);

        if (documentState != null) return;

        documentState = new CaseDurableEntityDocumentsState()
        {
            CmsDocuments = new List<CmsDocumentEntity>()
            {
                new CmsDocumentEntity(
                    cmsDocumentId: cmsDocumentDto.DocumentId,
                    versionId: cmsDocumentDto.VersionId,
                    cmsDocType: cmsDocumentDto.CmsDocType,
                    path: cmsDocumentDto.Path,
                    cmsFileCreatedDate: cmsDocumentDto.DocumentDate,
                    cmsOriginalFileName: cmsDocumentDto.FileName,
                    presentationTitle: cmsDocumentDto.PresentationTitle,
                    isOcrProcessed: cmsDocumentDto.IsOcrProcessed,
                    isDispatched: cmsDocumentDto.IsDispatched,
                    categoryListOrder: cmsDocumentDto.CategoryListOrder,
                    cmsParentDocumentId: cmsDocumentDto.ParentDocumentId,
                    witnessId: cmsDocumentDto.WitnessId,
                    presentationFlags: cmsDocumentDto.PresentationFlags,
                    hasFailedAttachments: cmsDocumentDto.HasFailedAttachments,
                    hasNotes: cmsDocumentDto.HasNotes,
                    isUnused: cmsDocumentDto.IsUnused,
                    isInbox: cmsDocumentDto.IsInbox,
                    classification: cmsDocumentDto.Classification,
                    isWitnessManagement: cmsDocumentDto.IsWitnessManagement,
                    canReclassify: cmsDocumentDto.CanReclassify,
                    canRename: cmsDocumentDto.CanRename,
                    renameStatus: cmsDocumentDto.RenameStatus,
                    reference: cmsDocumentDto.Reference),
            },
        };

        await this.polarisBlobStorageService.UploadObjectAsync(documentState, documentsStateBlobId);
    }
}
