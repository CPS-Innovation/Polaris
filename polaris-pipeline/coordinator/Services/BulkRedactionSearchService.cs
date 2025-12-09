using Common.Configuration;
using Common.Domain.Document;
using Common.Domain.Ocr;
using Common.Dto.Response.Document;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Services.BlobStorage.Factories;
using coordinator.Builders;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.Durable.Providers;
using coordinator.Enums;
using coordinator.OcrDocumentSearch;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Services;

public class BulkRedactionSearchService : IBulkRedactionSearchService
{
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;
    private readonly IBulkRedactionSearchResponseBuilder _bulkRedactionSearchResponseBuilder;
    private readonly IOcrDocumentSearch _ocrDocumentSearch;
    private readonly IMdsClient _mdsClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;

    public BulkRedactionSearchService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOrchestrationProvider orchestrationProvider, IBulkRedactionSearchResponseBuilder bulkRedactionSearchResponseBuilder, IOcrDocumentSearch ocrDocumentSearch, IConfiguration configuration, IMdsClient mdsClient, IDdeiArgFactory ddeiArgFactory)
    {
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        _orchestrationProvider = orchestrationProvider.ExceptionIfNull();
        _bulkRedactionSearchResponseBuilder = bulkRedactionSearchResponseBuilder.ExceptionIfNull();
        _ocrDocumentSearch = ocrDocumentSearch.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
    }

    public async Task<BulkRedactionSearchResponse> BulkRedactionSearchAsync(string urn, int caseId, string documentId, long versionId, string searchText, DurableTaskClient orchestrationClient, string cmsAuthValues, Guid correlationId, CancellationToken cancellationToken)
    {
        var documentType = DocumentNature.GetDocumentNatureType(documentId);

        if (documentType != DocumentNature.Types.Document)
        {
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("Document is not redactable")
                .Build(urn, caseId, documentId, versionId, searchText);
        }

        var caseIdentifiersArg = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);
        var listDocumentResponse = await _mdsClient.ListDocumentsAsync(caseIdentifiersArg);

        var cmsDocumentDto = listDocumentResponse.FirstOrDefault(x => documentId.Contains(x.DocumentId.ToString()) && x.VersionId == versionId);

        if (cmsDocumentDto is null)
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("Document not found in list document", true)
                .Build(urn, caseId, documentId, versionId, searchText);

        var documentPayload = CreateDocumentPayload(urn, caseId, documentId, versionId, cmsDocumentDto, cmsAuthValues, correlationId);

        await SetDocumentStateAsync(cmsDocumentDto, caseId);

        var orchestrationProviderStatus = await _orchestrationProvider.BulkSearchDocumentAsync(orchestrationClient, documentPayload, cancellationToken);

        switch (orchestrationProviderStatus)
        {
            case OrchestrationProviderStatuses.Initiated:
                return _bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshInitiated()
                    .Build(urn, caseId, documentId, versionId, searchText);
            case OrchestrationProviderStatuses.Processing:
                return _bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshProcessing()
                    .Build(urn, caseId, documentId, versionId, searchText);
            case OrchestrationProviderStatuses.Failed:
                return _bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Orchestration failure")
                    .Build(urn, caseId, documentId, versionId, searchText);
        }

        var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.Ocr);
        var results = await _polarisBlobStorageService.TryGetObjectAsync<AnalyzeResults>(blobId);
        if (results is null)
        {
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("OCR Document Not Found", true)
                .Build(urn, caseId, documentId, versionId, searchText);
        }

        var ocrDocumentSearchResponse = _ocrDocumentSearch.Search(searchText, results);

        if (!string.IsNullOrEmpty(ocrDocumentSearchResponse.FailureReason))
        {
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed(ocrDocumentSearchResponse.FailureReason)
                .Build(urn, caseId, documentId, versionId, searchText);
        }

        return _bulkRedactionSearchResponseBuilder
            .BuildDocumentRefreshCompleted()
            .BuildRedactionDefinitions(ocrDocumentSearchResponse.redactionDefinitionDtos)
            .Build(urn, caseId, documentId, versionId, searchText);
    }

    private DocumentPayload CreateDocumentPayload(string urn, int caseId, string documentId, long versionId, CmsDocumentDto cmsDocumentDto, string cmsAuthValues, Guid correlationId)
    {
        return new DocumentPayload
        {
            Urn = urn,
            CaseId = caseId,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = correlationId,
            DocumentId = documentId,
            VersionId = versionId,
            Path = cmsDocumentDto.Path,
            DocumentType = cmsDocumentDto.CmsDocType,
            DocumentNatureType = DocumentNature.Types.Document,
            DocumentDeltaType = DocumentDeltaType.RequiresIndexing,
            IsOcredProcessedPreference = cmsDocumentDto.IsOcrProcessed
        };
    }

    private async Task SetDocumentStateAsync(CmsDocumentDto cmsDocumentDto, int caseId)
    {
        var documentsStateBlobId = new BlobIdType(caseId, default, default, BlobType.DocumentState);
        var documentState = await _polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(documentsStateBlobId);

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
                    reference: cmsDocumentDto.Reference)
            }
        };

        await _polarisBlobStorageService.UploadObjectAsync(documentState, documentsStateBlobId);
    }
}