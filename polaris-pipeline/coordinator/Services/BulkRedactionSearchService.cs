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

    public async Task<BulkRedactionSearchResponse> BulkRedactionSearchAsync(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken)
    {
        var documentType = DocumentNature.GetDocumentNatureType(bulkRedactionSearchDto.DocumentId);

        if (documentType != DocumentNature.Types.Document)
        {
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("Document is not redactable")
                .Build(bulkRedactionSearchDto);
        }

        var caseIdentifiersArg = _ddeiArgFactory.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId);
        var listDocumentResponse = await _mdsClient.ListDocumentsAsync(caseIdentifiersArg);

        var cmsDocumentDto = listDocumentResponse.FirstOrDefault(x => bulkRedactionSearchDto.DocumentId.Contains(x.DocumentId.ToString()) && x.VersionId == bulkRedactionSearchDto.VersionId);

        if (cmsDocumentDto is null)
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("Document not found in list document", true)
                .Build(bulkRedactionSearchDto);

        var documentPayload = CreateDocumentPayload(bulkRedactionSearchDto, cmsDocumentDto);

        await SetDocumentStateAsync(cmsDocumentDto, bulkRedactionSearchDto.CaseId);

        var orchestrationProviderStatus = await _orchestrationProvider.BulkSearchDocumentAsync(orchestrationClient, documentPayload, cancellationToken);

        switch (orchestrationProviderStatus)
        {
            case OrchestrationProviderStatuses.Initiated:
                return _bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshInitiated()
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatuses.Processing:
                return _bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshProcessing()
                    .Build(bulkRedactionSearchDto);
            case OrchestrationProviderStatuses.Failed:
                return _bulkRedactionSearchResponseBuilder
                    .BuildDocumentRefreshFailed("Orchestration failure")
                    .Build(bulkRedactionSearchDto);
        }

        var blobId = new BlobIdType(bulkRedactionSearchDto.CaseId, bulkRedactionSearchDto.DocumentId, bulkRedactionSearchDto.VersionId, BlobType.Ocr);
        var results = await _polarisBlobStorageService.TryGetObjectAsync<AnalyzeResults>(blobId);
        if (results is null)
        {
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed("OCR Document Not Found", true)
                .Build(bulkRedactionSearchDto);
        }

        var ocrDocumentSearchResponse = _ocrDocumentSearch.Search(bulkRedactionSearchDto.SearchText, results);

        if (!string.IsNullOrEmpty(ocrDocumentSearchResponse.FailureReason))
        {
            return _bulkRedactionSearchResponseBuilder
                .BuildDocumentRefreshFailed(ocrDocumentSearchResponse.FailureReason)
                .Build(bulkRedactionSearchDto);
        }

        return _bulkRedactionSearchResponseBuilder
            .BuildDocumentRefreshCompleted()
            .BuildRedactionDefinitions(ocrDocumentSearchResponse.redactionDefinitionDtos)
            .Build(bulkRedactionSearchDto);
    }

    private DocumentPayload CreateDocumentPayload(BulkRedactionSearchDto bulkRedactionSearchDto, CmsDocumentDto cmsDocumentDto)
    {
        return new DocumentPayload
        {
            Urn = bulkRedactionSearchDto.Urn,
            CaseId = bulkRedactionSearchDto.CaseId,
            CmsAuthValues = bulkRedactionSearchDto.CmsAuthValues,
            CorrelationId = bulkRedactionSearchDto.CorrelationId,
            DocumentId = bulkRedactionSearchDto.DocumentId,
            VersionId = bulkRedactionSearchDto.VersionId,
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