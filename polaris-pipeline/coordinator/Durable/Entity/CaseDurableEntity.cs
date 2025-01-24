using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using coordinator.Domain;
using Microsoft.Extensions.Configuration;
using Common.Services.BlobStorage;
using Common.Configuration;

namespace coordinator.Durable.Entity;

public class CaseDurableEntity : TaskEntity<CaseDurableEntityState>, ICaseDurableEntity
{
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;

    public CaseDurableEntity(IConfiguration configuration, Func<string, IPolarisBlobStorageService> blobStorageServiceFactory)
    {
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
    }

    public static string GetKey(int caseId) => $"[{caseId}]";

    public static EntityInstanceId GetEntityId(int caseId) => new(nameof(CaseDurableEntity), GetKey(caseId));

    public static string GetEntityInstanceIdString(int caseId) => $"@{nameof(CaseDurableEntity)}@{GetKey(caseId)}";

    [Function(nameof(CaseDurableEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher taskEntityDispatcher) =>
        taskEntityDispatcher.DispatchAsync<CaseDurableEntity>();

    public Task<DateTime> GetStartTime() => Task.FromResult(State.Running.GetValueOrDefault());

    public void SetCaseId(int caseId) => State.CaseId = caseId;

    public void Reset()
    {
        State.Status = CaseRefreshStatus.NotStarted;
        State.Running = null;
        State.Retrieved = null;
        State.Completed = null;
        State.Failed = null;
        State.FailedReason = null;
    }

    public async Task<CaseDurableEntityDocumentsState> GetDurableEntityDocumentsStateAsync()
    {
        var blobId = new BlobIdType(State.CaseId, default, default, BlobType.DocumentList);

        return (await _polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(blobId)) ?? new CaseDurableEntityDocumentsState();
    }

    public async Task<bool> UpdateDurableEntityDocumentsStateAsync(CaseDurableEntityDocumentsState caseDurableEntityDocumentsState)
    {
        var blobId = new BlobIdType(State.CaseId, default, default, BlobType.DocumentList);
        await _polarisBlobStorageService.UploadObjectAsync(caseDurableEntityDocumentsState, blobId);

        return true;
    }
    /*
    public void Reset()
    {
        State.Status = CaseRefreshStatus.NotStarted;
        State.Running = null;
        State.Retrieved = null;
        State.Completed = null;
        State.Failed = null;
        State.FailedReason = null;
    }

    public void Reset()
    {
        State.Status = CaseRefreshStatus.NotStarted;
        State.Running = null;
        State.Retrieved = null;
        State.Completed = null;
        State.Failed = null;
        State.FailedReason = null;
    }
    */
    public async Task<CaseDeltasEntity> GetCaseDocumentChanges(GetCaseDocumentsResponse getCaseDocumentsResponse)
    {
        var cmsDocuments = getCaseDocumentsResponse.CmsDocuments;
        var pcdRequests = getCaseDocumentsResponse.PcdRequests;
        var defendantsAndCharges = getCaseDocumentsResponse.DefendantAndCharges;

        var (createdDocuments, updatedDocuments, deletedDocuments) = await GetDeltaCmsDocuments([.. cmsDocuments]);
        var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = await GetDeltaPcdRequestsAsync([.. pcdRequests]);
        var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = await GetDeltaDefendantsAndChargesAsync(defendantsAndCharges);

        var deltas = new CaseDeltasEntity
        {
            CreatedCmsDocuments = await CreateTrackerCmsDocumentsAsync(createdDocuments),
            UpdatedCmsDocuments = await UpdateTrackerCmsDocumentsAsync(updatedDocuments),
            DeletedCmsDocuments = await DeleteTrackerCmsDocumentsAsync(deletedDocuments),
            CreatedPcdRequests = await CreateTrackerPcdRequestsAsync(createdPcdRequests),
            UpdatedPcdRequests = await UpdateTrackerPcdRequestsAsync(updatedPcdRequests),
            DeletedPcdRequests = await DeleteTrackerPcdRequestsAsync(deletedPcdRequests),
            CreatedDefendantsAndCharges = await CreateTrackerDefendantsAndChargesAsync(createdDefendantsAndCharges),
            UpdatedDefendantsAndCharges = await UpdateTrackerDefendantsAndChargesAsync(updatedDefendantsAndCharges),
            IsDeletedDefendantsAndCharges = await DeleteTrackerDefendantsAndChargesAsync(deletedDefendantsAndCharges),
        };

        return await Task.FromResult(deltas);
    }

    private async Task<(List<CmsDocumentDto>, List<CmsDocumentDto>, List<long>)> GetDeltaCmsDocuments(List<CmsDocumentDto> incomingDocuments)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var newDocuments =
            (from incomingDocument in incomingDocuments
             let cmsDocument = documentsState.CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
             where cmsDocument == null
             select incomingDocument).ToList();

        var updatedDocuments =
            (from incomingDocument in incomingDocuments
             let cmsDocument = documentsState.CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
             where
             (
                 cmsDocument != null &&
                 (
                     cmsDocument.VersionId != incomingDocument.VersionId ||
                     cmsDocument.IsOcrProcessed != incomingDocument.IsOcrProcessed ||
                     cmsDocument.CmsDocType?.DocumentTypeId != incomingDocument.CmsDocType?.DocumentTypeId ||
                     cmsDocument.CmsDocType?.DocumentCategory != incomingDocument.CmsDocType?.DocumentCategory ||
                     cmsDocument.PresentationTitle != incomingDocument.PresentationTitle ||
                     cmsDocument.CategoryListOrder != incomingDocument.CategoryListOrder ||
                     cmsDocument.WitnessId != incomingDocument.WitnessId ||
                     cmsDocument.CmsFileCreatedDate != incomingDocument.DocumentDate ||
                     cmsDocument.IsDispatched != incomingDocument.IsDispatched ||
                     cmsDocument.HasNotes != incomingDocument.HasNotes ||
                     cmsDocument.IsUnused != incomingDocument.IsUnused ||
                     cmsDocument.IsInbox != incomingDocument.IsInbox ||
                     cmsDocument.CanReclassify != incomingDocument.CanReclassify ||
                     cmsDocument.Reference != incomingDocument.Reference))
             select incomingDocument).ToList();

        // todo: filetype check here
        var deletedCmsDocumentIdsToRemove
            = documentsState.CmsDocuments.Where(doc => !incomingDocuments.Any(incomingDoc => incomingDoc.DocumentId == doc.CmsDocumentId))
                .Select(doc => doc.CmsDocumentId)
                .ToList();

        return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
    }

    private async Task<(List<PcdRequestCoreDto> createdPcdRequests, List<PcdRequestCoreDto> updatedPcdRequests, List<long> deletedPcdRequests)> GetDeltaPcdRequestsAsync(List<PcdRequestCoreDto> incomingPcdRequests)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var newPcdRequests =
            incomingPcdRequests
                .Where(incomingPcd => !documentsState.PcdRequests.Any(pcd => pcd.CmsDocumentId == incomingPcd.Id))
                .ToList();

        // Return empty list for updated pcds.  Before this we had the following:
        //     .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != DocumentStatus.Indexed))
        // which did nothing.  We could be doing some sort of hash comparison here on pcd requests to see if they've changed, but this would
        // involve always having to request the full pcd request from DDEI on every refresh, which would be costly.
        var updatedPcdRequests = Enumerable.Empty<PcdRequestCoreDto>().ToList();

        var deletedPcdRequestIds
            = documentsState.PcdRequests.Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.CmsDocumentId))
                .Select(pcd => pcd.CmsDocumentId)
                .ToList();

        return (newPcdRequests, updatedPcdRequests, deletedPcdRequestIds);
    }

    private async Task<(DefendantsAndChargesListDto createdDefendantsAndCharges, DefendantsAndChargesListDto updatedDefendantsAndCharges, bool deletedDefendantsAndCharges)> GetDeltaDefendantsAndChargesAsync(DefendantsAndChargesListDto incomingDefendantsAndCharges)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        DefendantsAndChargesListDto newDefendantsAndCharges = null, updatedDefendantsAndCharges = null;

        if (documentsState.DefendantsAndCharges == null && incomingDefendantsAndCharges != null)
        {
            newDefendantsAndCharges = incomingDefendantsAndCharges;
        }

        if (documentsState.DefendantsAndCharges != null && incomingDefendantsAndCharges != null)
        {
            if (documentsState.DefendantsAndCharges.VersionId != incomingDefendantsAndCharges.VersionId)
            {
                updatedDefendantsAndCharges = incomingDefendantsAndCharges;
            }
        }

        var deletedDefendantsAndCharges = documentsState.DefendantsAndCharges != null && incomingDefendantsAndCharges == null;

        return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
    }

    private async Task<List<DocumentDelta>> CreateTrackerCmsDocumentsAsync(List<CmsDocumentDto> createdDocuments)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var newDocuments = new List<DocumentDelta>();

        foreach (var newDocument in createdDocuments)
        {
            var trackerDocument = new CmsDocumentEntity(
                    cmsDocumentId: newDocument.DocumentId,
                    versionId: newDocument.VersionId,
                    cmsDocType: newDocument.CmsDocType,
                    path: newDocument.Path,
                    cmsFileCreatedDate: newDocument.DocumentDate,
                    cmsOriginalFileName: newDocument.FileName,
                    presentationTitle: newDocument.PresentationTitle,
                    isOcrProcessed: newDocument.IsOcrProcessed,
                    isDispatched: newDocument.IsDispatched,
                    categoryListOrder: newDocument.CategoryListOrder,
                    cmsParentDocumentId: newDocument.ParentDocumentId,
                    witnessId: newDocument.WitnessId,
                    presentationFlags: newDocument.PresentationFlags,
                    hasFailedAttachments: newDocument.HasFailedAttachments,
                    hasNotes: newDocument.HasNotes,
                    isUnused: newDocument.IsUnused,
                    isInbox: newDocument.IsInbox,
                    classification: newDocument.Classification,
                    isWitnessManagement: newDocument.IsWitnessManagement,
                    canReclassify: newDocument.CanReclassify,
                    canRename: newDocument.CanRename,
                    renameStatus: newDocument.RenameStatus,
                    reference: newDocument.Reference
                );

            documentsState.CmsDocuments.Add(trackerDocument);
            await UpdateDurableEntityDocumentsStateAsync(documentsState);
            newDocuments.Add(new DocumentDelta { Document = trackerDocument, DeltaType = DocumentDeltaType.RequiresIndexing });
        }

        return newDocuments;
    }

    private async Task<List<DocumentDelta>> UpdateTrackerCmsDocumentsAsync(List<CmsDocumentDto> updatedDocuments)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var changedDocuments = new List<DocumentDelta>();

        foreach (var updatedDocument in updatedDocuments)
        {
            var trackerDocument = documentsState.CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

            trackerDocument.CmsDocType = updatedDocument.CmsDocType;
            trackerDocument.Path = updatedDocument.Path;
            trackerDocument.CmsFileCreatedDate = updatedDocument.DocumentDate;
            trackerDocument.PresentationTitle = updatedDocument.PresentationTitle;
            trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;
            trackerDocument.IsDispatched = updatedDocument.IsDispatched;
            trackerDocument.CmsParentDocumentId = updatedDocument.ParentDocumentId;
            trackerDocument.WitnessId = updatedDocument.WitnessId;
            trackerDocument.CategoryListOrder = updatedDocument.CategoryListOrder;
            trackerDocument.HasNotes = updatedDocument.HasNotes;
            trackerDocument.IsInbox = updatedDocument.IsInbox;
            trackerDocument.IsUnused = updatedDocument.IsUnused;
            trackerDocument.Classification = updatedDocument.Classification;
            trackerDocument.IsWitnessManagement = updatedDocument.IsWitnessManagement;
            trackerDocument.CanReclassify = updatedDocument.CanReclassify;
            trackerDocument.CanRename = updatedDocument.CanRename;
            trackerDocument.RenameStatus = updatedDocument.RenameStatus;
            trackerDocument.Reference = updatedDocument.Reference;

            var caseDeltaType = DocumentDeltaType.DoesNotRequireRefresh;

            if (trackerDocument.IsOcrProcessed != updatedDocument.IsOcrProcessed)
            {
                trackerDocument.IsOcrProcessed = updatedDocument.IsOcrProcessed;
                caseDeltaType = DocumentDeltaType.RequiresPdfRefresh;
            }

            if (trackerDocument.VersionId != updatedDocument.VersionId)
            {
                trackerDocument.VersionId = updatedDocument.VersionId;
                caseDeltaType = DocumentDeltaType.RequiresIndexing;
            }

            changedDocuments.Add(new DocumentDelta { Document = trackerDocument, DeltaType = caseDeltaType });
        }

        await UpdateDurableEntityDocumentsStateAsync(documentsState);
        return changedDocuments
            .Where(d => d.DeltaType != DocumentDeltaType.DoesNotRequireRefresh)
            .ToList();
    }

    private async Task<List<CmsDocumentEntity>> DeleteTrackerCmsDocumentsAsync(List<long> documentIdsToDelete)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var deleteDocuments = documentsState.CmsDocuments
            .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
            .ToList();

        foreach (var document in deleteDocuments)
        {
            documentsState.CmsDocuments.Remove(document);
        }

        await UpdateDurableEntityDocumentsStateAsync(documentsState);
        return deleteDocuments;
    }

    private async Task<List<PcdRequestEntity>> CreateTrackerPcdRequestsAsync(List<PcdRequestCoreDto> createdPcdRequests)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var newPcdRequests = new List<PcdRequestEntity>();

        foreach (var newPcdRequest in createdPcdRequests)
        {

            var trackerPcdRequest = new PcdRequestEntity(newPcdRequest.Id, 1, newPcdRequest);
            documentsState.PcdRequests.Add(trackerPcdRequest);
            newPcdRequests.Add(trackerPcdRequest);
        }

        await UpdateDurableEntityDocumentsStateAsync(documentsState);
        return newPcdRequests;
    }

    private async Task<List<PcdRequestEntity>> UpdateTrackerPcdRequestsAsync(List<PcdRequestCoreDto> updatedPcdRequests)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var changedPcdRequests = new List<PcdRequestEntity>();

        foreach (var updatedPcdRequest in updatedPcdRequests)
        {
            var trackerPcdRequest = documentsState.PcdRequests.Find(pcd => pcd.CmsDocumentId == updatedPcdRequest.Id);
            trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

            changedPcdRequests.Add(trackerPcdRequest);
        }

        await UpdateDurableEntityDocumentsStateAsync(documentsState);
        return changedPcdRequests;
    }

    private async Task<List<PcdRequestEntity>> DeleteTrackerPcdRequestsAsync(List<long> deletedPcdRequestIds)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var deletePcdRequests
            = documentsState.PcdRequests
                .Where(pcd => deletedPcdRequestIds.Contains(pcd.CmsDocumentId))
                .ToList();

        foreach (var pcdRequest in deletePcdRequests)
        {
            documentsState.PcdRequests.Remove(pcdRequest);
        }

        await UpdateDurableEntityDocumentsStateAsync(documentsState);
        return deletePcdRequests;
    }

    private async Task<DefendantsAndChargesEntity> CreateTrackerDefendantsAndChargesAsync(DefendantsAndChargesListDto createdDefendantsAndCharges)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        if (createdDefendantsAndCharges != null)
        {
            documentsState.DefendantsAndCharges = new DefendantsAndChargesEntity(
                createdDefendantsAndCharges.CaseId,
                createdDefendantsAndCharges.VersionId,
                createdDefendantsAndCharges);

            await UpdateDurableEntityDocumentsStateAsync(documentsState);
            return documentsState.DefendantsAndCharges;
        }

        return null;
    }

    private async Task<DefendantsAndChargesEntity> UpdateTrackerDefendantsAndChargesAsync(DefendantsAndChargesListDto updatedDefendantsAndCharges)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        if (updatedDefendantsAndCharges != null)
        {
            // todo: encapsulate this logic into DefendantsAndChargesEntity
            documentsState.DefendantsAndCharges.HasMultipleDefendants = updatedDefendantsAndCharges?.DefendantsAndCharges.Count() > 1;
            await UpdateDurableEntityDocumentsStateAsync(documentsState); 

            return documentsState.DefendantsAndCharges;
        }

        return null;
    }

    private async Task<bool> DeleteTrackerDefendantsAndChargesAsync(bool deletedDefendantsAndCharges)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        if (deletedDefendantsAndCharges)
        {
            documentsState.DefendantsAndCharges = null;
        }

        await UpdateDurableEntityDocumentsStateAsync(documentsState);
        return deletedDefendantsAndCharges;
    }

    private BaseDocumentEntity GetDocument(string documentId, CaseDurableEntityDocumentsState documentsState)
    {
        var cmsDocument = documentsState.CmsDocuments.Find(doc => doc.DocumentId == documentId);
        if (cmsDocument != null)
        {
            return cmsDocument;
        }

        var pcdRequest = documentsState.PcdRequests.Find(pcd => pcd.DocumentId == documentId);
        if (pcdRequest != null)
        {
            return pcdRequest;
        }

        if (documentsState.DefendantsAndCharges != null)
        {
            return documentsState.DefendantsAndCharges;
        }

        return null;
    }

    public void SetCaseStatus(SetCaseStatusPayload payload)
    {
        State.Status = payload.Status;

        switch (State.Status)
        {
            case CaseRefreshStatus.Running:
                State.Running = payload.UpdatedAt;
                break;

            case CaseRefreshStatus.DocumentsRetrieved:
                if (State.Running != null)
                {
                    State.Retrieved = (float)((payload.UpdatedAt - State.Running).Value.TotalMilliseconds / 1000.0);
                }

                break;

            case CaseRefreshStatus.Completed:
                if (State.Running != null)
                {
                    State.Completed = (float)((payload.UpdatedAt - State.Running).Value.TotalMilliseconds / 1000.0);
                }

                break;

            case CaseRefreshStatus.Failed:
                if (State.Running != null)
                {
                    State.Failed = (float)((payload.UpdatedAt - State.Running).Value.TotalMilliseconds / 1000.0);
                    State.FailedReason = payload.FailedReason;
                }
                break;
        }
    }

    public async Task<bool> SetDocumentPdfConversionSucceededAsync(string documentId)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var document = GetDocument(documentId, documentsState);
        document.Status = DocumentStatus.PdfUploadedToBlob;

        return await UpdateDurableEntityDocumentsStateAsync(documentsState);
    }

    public async Task<bool> SetDocumentPdfConversionFailedAsync(SetDocumentPdfConversionFailedPayload payload)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var document = GetDocument(payload.DocumentId, documentsState);
        document.Status = DocumentStatus.UnableToConvertToPdf;
        document.ConversionStatus = payload.PdfConversionStatus;

        return await UpdateDurableEntityDocumentsStateAsync(documentsState);
    }

    public async Task<bool> SetDocumentIndexingSucceededAsync(string documentId)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var document = GetDocument(documentId, documentsState);
        document.Status = DocumentStatus.Indexed;

        return await UpdateDurableEntityDocumentsStateAsync(documentsState);
    }

    public async Task<bool> SetDocumentIndexingFailedAsync(string documentId)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var document = GetDocument(documentId, documentsState);
        document.Status = DocumentStatus.OcrAndIndexFailure;

        return await UpdateDurableEntityDocumentsStateAsync(documentsState);
    }

    public async Task<bool> SetPiiVersionIdAsync(string documentId)
    {
        var documentsState = await GetDurableEntityDocumentsStateAsync();
        var document = GetDocument(documentId, documentsState);
        document.PiiVersionId = document.VersionId;

        return await UpdateDurableEntityDocumentsStateAsync(documentsState);
    }
}