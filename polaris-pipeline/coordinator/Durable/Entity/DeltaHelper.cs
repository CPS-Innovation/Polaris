using System.Collections.Generic;
using System.Linq;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.ValueObjects;
using coordinator.Durable.Payloads.Domain;
using Newtonsoft.Json;

namespace coordinator.Durable.Entity;

public static class DeltaHelper
{
    public CaseDeltasEntity MutateAndReturnDocumentDeltas(List<CmsDocumentDto> cmsDocuments, List<PcdRequestDto> pcdRequests, DefendantsAndChargesListDto defendantsAndCharges, CaseDurableEntity entity)
    {
        var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(
            existingDocs: entity.CmsDocuments,
            nextDocs: cmsDocuments
        );

        var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(
            existingPcds: entity.PcdRequests,
            nextPcds: pcdRequests
        );

        var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = GetDeltaDefendantsAndCharges(
            existingDac: entity.DefendantsAndCharges,
            nextDac: defendantsAndCharges
        );

        return new CaseDeltasEntity
        {
            CreatedCmsDocuments = CreateTrackerCmsDocuments(createdDocuments),
            UpdatedCmsDocuments = UpdateTrackerCmsDocuments(updatedDocuments),
            DeletedCmsDocuments = DeleteTrackerCmsDocuments(deletedDocuments),
            CreatedPcdRequests = CreateTrackerPcdRequests(createdPcdRequests),
            UpdatedPcdRequests = UpdateTrackerPcdRequests(updatedPcdRequests),
            DeletedPcdRequests = DeleteTrackerPcdRequests(deletedPcdRequests),
            CreatedDefendantsAndCharges = CreateTrackerDefendantsAndCharges(createdDefendantsAndCharges),
            UpdatedDefendantsAndCharges = UpdateTrackerDefendantsAndCharges(updatedDefendantsAndCharges),
            IsDeletedDefendantsAndCharges = DeleteTrackerDefendantsAndCharges(deletedDefendantsAndCharges),
        };
    }

    public static bool HasDocumentChanged(CmsDocumentDto newDoc, CmsDocumentEntity existingDoc)
    {
        return existingDoc.CmsVersionId != newDoc.VersionId
               || existingDoc.IsOcrProcessed != newDoc.IsOcrProcessed
               || existingDoc.CmsDocType?.DocumentTypeId != newDoc.CmsDocType?.DocumentTypeId
               || existingDoc.CmsDocType?.DocumentCategory != newDoc.CmsDocType?.DocumentCategory
               || existingDoc.PresentationTitle != newDoc.PresentationTitle
               || existingDoc.CategoryListOrder != newDoc.CategoryListOrder
               || existingDoc.WitnessId != newDoc.WitnessId
               || existingDoc.CmsFileCreatedDate != newDoc.DocumentDate
               || existingDoc.IsDispatched != newDoc.IsDispatched
               || existingDoc.HasNotes != newDoc.HasNotes;
    }

    private static (List<CmsDocumentDto>, List<CmsDocumentDto>, List<string>) GetDeltaCmsDocuments(
        List<CmsDocumentEntity> existingDocs,
        List<CmsDocumentDto> nextDocs
    )
    {
        var newDocuments = nextDocs
            .Where(nextDoc => !existingDocs.Any(existingDoc => existingDoc.CmsDocumentId == nextDoc.DocumentId))
            .ToList();

        var updatedDocuments = nextDocs
            .Join(existingDocs,
                newDoc => newDoc.DocumentId,
                existingDoc => existingDoc.CmsDocumentId,
                (newDoc, existingDoc) => (newDoc, existingDoc)
            )
            .Where(pair => HasDocumentChanged(pair.newDoc, pair.existingDoc))
            .Select(pair => pair.newDoc)
            .ToList();

        // todo: filetype check here
        var deletedCmsDocumentIdsToRemove
            = existingDocs.Where(doc => !nextDocs.Any(incomingDoc => incomingDoc.DocumentId == doc.CmsDocumentId))
                .Select(doc => doc.CmsDocumentId)
                .ToList();

        return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
    }

    private static (List<PcdRequestDto> createdPcdRequests, List<PcdRequestDto> updatedPcdRequests, List<int> deletedPcdRequests) GetDeltaPcdRequests(
        List<PcdRequestEntity> existingPcds,
        List<PcdRequestDto> nextPcds)
    {
        var newPcdRequests =
            nextPcds
                .Where(nextPcd => !existingPcds.Any(existingPcd => existingPcd.PcdRequest.Id == nextPcd.Id))
                .ToList();

        var updatedPcdRequests =
            nextPcds
                .Where(nextPcd => existingPcds.Any(existingPcd => existingPcd.PcdRequest.Id == nextPcd.Id
                    // todo: this is incorrect: all this does is say that we should only process if we are not already indexed i.e. we've failed on the pcd doc some time before successful indexing
                    //  The retry functionality is not a concern for here, and we probably need a better method to detect if a pcd has changed (if they can change at all?)
                    && existingPcd.Status != DocumentStatus.Indexed))
                .ToList();

        var deletedPcdRequestIds
            = existingPcds.Where(existingPcd => !nextPcds.Exists(nextPcd => nextPcd.Id == existingPcd.PcdRequest.Id))
                .Select(pcd => pcd.PcdRequest.Id)
                .ToList();

        return (newPcdRequests, updatedPcdRequests, deletedPcdRequestIds);
    }

    private static (DefendantsAndChargesListDto createdDefendantsAndCharges, DefendantsAndChargesListDto updatedDefendantsAndCharges, bool deletedDefendantsAndCharges) GetDeltaDefendantsAndCharges(
        DefendantsAndChargesEntity existingDac,
        DefendantsAndChargesListDto nextDac)
    {
        DefendantsAndChargesListDto newDefendantsAndCharges = null, updatedDefendantsAndCharges = null;

        if (existingDac == null && nextDac != null)
            newDefendantsAndCharges = nextDac;

        if (existingDac != null && nextDac != null)
        {
            // todo: a better mechanism here 
            if (JsonConvert.SerializeObject(existingDac.DefendantsAndCharges) != JsonConvert.SerializeObject(nextDac))
                updatedDefendantsAndCharges = nextDac;
        }

        var deletedDefendantsAndCharges = existingDac != null && nextDac == null;

        return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
    }

    private static List<(CmsDocumentEntity, DocumentDeltaType)> CreateTrackerCmsDocuments(List<CmsDocumentDto> newDocuments)
    {
        return newDocuments
            .Select(newDocument => new CmsDocumentEntity
                (
                    polarisDocumentId: new PolarisDocumentId(PolarisDocumentType.CmsDocument, newDocument.DocumentId),
                    polarisDocumentVersionId: 1,
                    cmsDocumentId: newDocument.DocumentId,
                    cmsVersionId: newDocument.VersionId,
                    cmsDocType: newDocument.CmsDocType,
                    path: newDocument.Path,
                    fileExtension: newDocument.FileExtension,
                    cmsFileCreatedDate: newDocument.DocumentDate,
                    cmsOriginalFileName: newDocument.FileName,
                    presentationTitle: newDocument.PresentationTitle,
                    isOcrProcessed: newDocument.IsOcrProcessed,
                    isDispatched: newDocument.IsDispatched,
                    categoryListOrder: newDocument.CategoryListOrder,
                    polarisParentDocumentId: new PolarisDocumentId(PolarisDocumentType.CmsDocument, newDocument.ParentDocumentId),
                    cmsParentDocumentId: newDocument.ParentDocumentId,
                    witnessId: newDocument.WitnessId,
                    presentationFlags: newDocument.PresentationFlags,
                    hasFailedAttachments: newDocument.HasFailedAttachments,
                    hasNotes: newDocument.HasNotes
                ))
            .Select(doc => (doc, DocumentDeltaType.RequiresIndexing))
            .ToList();
    }

    private List<(CmsDocumentEntity, DocumentDeltaType)> UpdateTrackerCmsDocuments(List<CmsDocumentDto> updatedDocuments)
    {
        var changedDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>();

        foreach (var updatedDocument in updatedDocuments)
        {
            var trackerDocument = CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

            trackerDocument.CmsDocType = updatedDocument.CmsDocType;
            trackerDocument.Path = updatedDocument.Path;
            trackerDocument.CmsOriginalFileExtension = updatedDocument.FileExtension;
            trackerDocument.CmsFileCreatedDate = updatedDocument.DocumentDate;
            trackerDocument.PresentationTitle = updatedDocument.PresentationTitle;
            trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;
            trackerDocument.IsDispatched = updatedDocument.IsDispatched;
            trackerDocument.CmsParentDocumentId = updatedDocument.ParentDocumentId;
            trackerDocument.WitnessId = updatedDocument.WitnessId;
            trackerDocument.CategoryListOrder = updatedDocument.CategoryListOrder;
            trackerDocument.HasNotes = updatedDocument.HasNotes;

            var caseDeltaType = DocumentDeltaType.DoesNotRequireRefresh;

            if (trackerDocument.IsOcrProcessed != updatedDocument.IsOcrProcessed)
            {
                trackerDocument.IsOcrProcessed = updatedDocument.IsOcrProcessed;
                caseDeltaType = DocumentDeltaType.RequiresPdfRefresh;
            }

            if (trackerDocument.CmsVersionId != updatedDocument.VersionId)
            {
                trackerDocument.PolarisDocumentVersionId++;
                trackerDocument.CmsVersionId = updatedDocument.VersionId;
                caseDeltaType = DocumentDeltaType.RequiresIndexing;
            }

            changedDocuments.Add((trackerDocument, caseDeltaType));
        }

        return changedDocuments
            .Where(d => d.Item2 != DocumentDeltaType.DoesNotRequireRefresh)
            .ToList();
    }

    private List<CmsDocumentEntity> DeleteTrackerCmsDocuments(List<string> documentIdsToDelete)
    {
        var deleteDocuments
            = CmsDocuments
                .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
                .ToList();

        foreach (var document in deleteDocuments)
        {
            CmsDocuments.Remove(document);
        }

        return deleteDocuments;
    }

    private List<PcdRequestEntity> CreateTrackerPcdRequests(List<PcdRequestDto> createdPcdRequests)
    {
        var newPcdRequests = new List<PcdRequestEntity>();

        foreach (var newPcdRequest in createdPcdRequests)
        {
            var polarisDocumentId = new PolarisDocumentId(PolarisDocumentType.PcdRequest, newPcdRequest.Id.ToString());
            var trackerPcdRequest = new PcdRequestEntity(polarisDocumentId, 1, newPcdRequest);
            PcdRequests.Add(trackerPcdRequest);
            newPcdRequests.Add(trackerPcdRequest);
        }

        return newPcdRequests;
    }

    private List<PcdRequestEntity> UpdateTrackerPcdRequests(List<PcdRequestDto> updatedPcdRequests)
    {
        var changedPcdRequests = new List<PcdRequestEntity>();

        foreach (var updatedPcdRequest in updatedPcdRequests)
        {
            var trackerPcdRequest = PcdRequests.Find(pcd => pcd.PcdRequest.Id == updatedPcdRequest.Id);

            trackerPcdRequest.PolarisDocumentVersionId++;
            trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

            changedPcdRequests.Add(trackerPcdRequest);
        }

        return changedPcdRequests;
    }

    private List<PcdRequestEntity> DeleteTrackerPcdRequests(List<int> deletedPcdRequestIds)
    {
        var deletePcdRequests
            = PcdRequests
                .Where(pcd => deletedPcdRequestIds.Contains(pcd.PcdRequest.Id))
                .ToList();

        foreach (var pcdRequest in deletePcdRequests)
        {
            PcdRequests.Remove(pcdRequest);
        }

        return deletePcdRequests;
    }

    private DefendantsAndChargesEntity CreateTrackerDefendantsAndCharges(DefendantsAndChargesListDto createdDefendantsAndCharges)
    {
        if (createdDefendantsAndCharges != null)
        {
            PolarisDocumentId polarisDocumentId = new PolarisDocumentId(PolarisDocumentType.DefendantsAndCharges, createdDefendantsAndCharges.CaseId.ToString());
            DefendantsAndCharges = new DefendantsAndChargesEntity(polarisDocumentId, 1, createdDefendantsAndCharges);

            return DefendantsAndCharges;
        }

        return null;
    }

    private DefendantsAndChargesEntity UpdateTrackerDefendantsAndCharges(DefendantsAndChargesListDto updatedDefendantsAndCharges)
    {
        if (updatedDefendantsAndCharges != null)
        {
            DefendantsAndCharges.DefendantsAndCharges = updatedDefendantsAndCharges;
            DefendantsAndCharges.PolarisDocumentVersionId++;

            return DefendantsAndCharges;
        }

        return null;
    }

    private bool DeleteTrackerDefendantsAndCharges(bool deletedDefendantsAndCharges)
    {
        if (deletedDefendantsAndCharges)
        {
            DefendantsAndCharges = null;
        }

        return deletedDefendantsAndCharges;
    }

}