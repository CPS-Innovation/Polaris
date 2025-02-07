using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;
using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class GetCaseDocumentChanges(IStateStorageService stateStorageService)
{
    [Function(nameof(GetCaseDocumentChanges))]
    public async Task<CaseDeltasEntity> RunAsync([ActivityTrigger] int caseId)
    {
        var getCaseDocumentsResponse = await stateStorageService.GetCaseDocumentsAsync(caseId);
        var documentsState = await stateStorageService.GetDurableEntityDocumentsStateAsync(caseId);

        var cmsDocuments = getCaseDocumentsResponse.CmsDocuments;
        var pcdRequests = getCaseDocumentsResponse.PcdRequests;
        var defendantsAndCharges = getCaseDocumentsResponse.DefendantAndCharges;

        var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments([.. cmsDocuments], documentsState);
        var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests([.. pcdRequests], documentsState);
        var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = GetDeltaDefendantsAndCharges(defendantsAndCharges, documentsState);

        var deltas = new CaseDeltasEntity
        {
            CreatedCmsDocuments = CreateTrackerCmsDocuments(createdDocuments, documentsState),
            UpdatedCmsDocuments = UpdateTrackerCmsDocuments(updatedDocuments, documentsState),
            DeletedCmsDocuments = DeleteTrackerCmsDocuments(deletedDocuments, documentsState),
            CreatedPcdRequests = CreateTrackerPcdRequests(createdPcdRequests, documentsState),
            UpdatedPcdRequests = UpdateTrackerPcdRequests(updatedPcdRequests, documentsState),
            DeletedPcdRequests = DeleteTrackerPcdRequests(deletedPcdRequests, documentsState),
            CreatedDefendantsAndCharges = CreateTrackerDefendantsAndCharges(createdDefendantsAndCharges, documentsState),
            UpdatedDefendantsAndCharges = UpdateTrackerDefendantsAndCharges(updatedDefendantsAndCharges, documentsState),
            IsDeletedDefendantsAndCharges = DeleteTrackerDefendantsAndCharges(deletedDefendantsAndCharges, documentsState),
        };

        await stateStorageService.UpdateCaseDeltasEntityAsync(caseId, deltas);
        await stateStorageService.UpdateDurableEntityDocumentsStateAsync(caseId, documentsState);

        return deltas;
    }


    private static (List<CmsDocumentDto>, List<CmsDocumentDto>, List<long>) GetDeltaCmsDocuments(List<CmsDocumentDto> incomingDocuments, CaseDurableEntityDocumentsState documentsState)
    {
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

    private static (List<PcdRequestCoreDto> createdPcdRequests, List<PcdRequestCoreDto> updatedPcdRequests, List<long> deletedPcdRequests) GetDeltaPcdRequests(
        List<PcdRequestCoreDto> incomingPcdRequests,
        CaseDurableEntityDocumentsState documentsState)
    {
        var newPcdRequests = incomingPcdRequests
            .Where(incomingPcd => !documentsState.PcdRequests.Any(pcd => pcd.CmsDocumentId == incomingPcd.Id))
            .ToList();

        // Return empty list for updated pcds.  Before this we had the following:
        //     .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != DocumentStatus.Indexed))
        // which did nothing.  We could be doing some sort of hash comparison here on pcd requests to see if they've changed, but this would
        // involve always having to request the full pcd request from DDEI on every refresh, which would be costly.
        var updatedPcdRequests = Enumerable.Empty<PcdRequestCoreDto>().ToList();

        var deletedPcdRequestIds = documentsState.PcdRequests
            .Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.CmsDocumentId))
            .Select(pcd => pcd.CmsDocumentId)
            .ToList();

        return (newPcdRequests, updatedPcdRequests, deletedPcdRequestIds);
    }

    private static (DefendantsAndChargesListDto createdDefendantsAndCharges, DefendantsAndChargesListDto updatedDefendantsAndCharges, bool deletedDefendantsAndCharges) GetDeltaDefendantsAndCharges(
        DefendantsAndChargesListDto incomingDefendantsAndCharges,
        CaseDurableEntityDocumentsState documentsState)
    {
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

    private static List<DocumentDelta> CreateTrackerCmsDocuments(List<CmsDocumentDto> createdDocuments, CaseDurableEntityDocumentsState documentsState)
    {
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
                    reference: newDocument.Reference);

            documentsState.CmsDocuments.Add(trackerDocument);
            newDocuments.Add(new DocumentDelta { Document = trackerDocument, DeltaType = DocumentDeltaType.RequiresIndexing });
        }

        return newDocuments;
    }

    private static List<DocumentDelta> UpdateTrackerCmsDocuments(List<CmsDocumentDto> updatedDocuments, CaseDurableEntityDocumentsState documentsState)
    {
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

        return changedDocuments
            .Where(d => d.DeltaType != DocumentDeltaType.DoesNotRequireRefresh)
            .ToList();
    }

    private static List<CmsDocumentEntity> DeleteTrackerCmsDocuments(List<long> documentIdsToDelete, CaseDurableEntityDocumentsState documentsState)
    {
        var deleteDocuments = documentsState.CmsDocuments
            .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
            .ToList();

        foreach (var document in deleteDocuments)
        {
            documentsState.CmsDocuments.Remove(document);
        }

        return deleteDocuments;
    }

    private static List<PcdRequestEntity> CreateTrackerPcdRequests(List<PcdRequestCoreDto> createdPcdRequests, CaseDurableEntityDocumentsState documentsState)
    {
        var newPcdRequests = new List<PcdRequestEntity>();

        foreach (var newPcdRequest in createdPcdRequests)
        {

            var trackerPcdRequest = new PcdRequestEntity(newPcdRequest.Id, 1, newPcdRequest);
            documentsState.PcdRequests.Add(trackerPcdRequest);
            newPcdRequests.Add(trackerPcdRequest);
        }

        return newPcdRequests;
    }

    private static List<PcdRequestEntity> UpdateTrackerPcdRequests(List<PcdRequestCoreDto> updatedPcdRequests, CaseDurableEntityDocumentsState documentsState)
    {
        var changedPcdRequests = new List<PcdRequestEntity>();

        foreach (var updatedPcdRequest in updatedPcdRequests)
        {
            var trackerPcdRequest = documentsState.PcdRequests.Find(pcd => pcd.CmsDocumentId == updatedPcdRequest.Id);
            trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

            changedPcdRequests.Add(trackerPcdRequest);
        }

        return changedPcdRequests;
    }

    private static List<PcdRequestEntity> DeleteTrackerPcdRequests(List<long> deletedPcdRequestIds, CaseDurableEntityDocumentsState documentsState)
    {
        var deletePcdRequests
            = documentsState.PcdRequests
                .Where(pcd => deletedPcdRequestIds.Contains(pcd.CmsDocumentId))
                .ToList();

        foreach (var pcdRequest in deletePcdRequests)
        {
            documentsState.PcdRequests.Remove(pcdRequest);
        }

        return deletePcdRequests;
    }

    private static DefendantsAndChargesEntity CreateTrackerDefendantsAndCharges(DefendantsAndChargesListDto createdDefendantsAndCharges, CaseDurableEntityDocumentsState documentsState)
    {
        if (createdDefendantsAndCharges != null)
        {
            documentsState.DefendantsAndCharges = new DefendantsAndChargesEntity(
                createdDefendantsAndCharges.CaseId,
                createdDefendantsAndCharges.VersionId,
                createdDefendantsAndCharges);

            return documentsState.DefendantsAndCharges;
        }

        return null;
    }

    private static DefendantsAndChargesEntity UpdateTrackerDefendantsAndCharges(DefendantsAndChargesListDto updatedDefendantsAndCharges, CaseDurableEntityDocumentsState documentsState)
    {
        if (updatedDefendantsAndCharges != null)
        {
            // todo: encapsulate this logic into DefendantsAndChargesEntity
            documentsState.DefendantsAndCharges.HasMultipleDefendants = updatedDefendantsAndCharges?.DefendantsAndCharges.Count() > 1;

            return documentsState.DefendantsAndCharges;
        }

        return null;
    }

    private static bool DeleteTrackerDefendantsAndCharges(bool deletedDefendantsAndCharges, CaseDurableEntityDocumentsState documentsState)
    {
        if (deletedDefendantsAndCharges)
        {
            documentsState.DefendantsAndCharges = null;
        }

        return deletedDefendantsAndCharges;
    }
}
