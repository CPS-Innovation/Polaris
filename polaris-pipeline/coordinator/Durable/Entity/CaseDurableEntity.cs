﻿using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Durable.Payloads.Domain;
using Common.Constants;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    [JsonObject(MemberSerialization.OptIn)]
    public class CaseDurableEntity : ICaseDurableEntity
    {
        public static string GetKey(int caseId) => $"[{caseId}]";

        public static EntityId GetEntityId(int caseId) => new EntityId(nameof(CaseDurableEntity), GetKey(caseId));

        [FunctionName(nameof(CaseDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseDurableEntity>();
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public CaseRefreshStatus Status { get; set; }

        [JsonProperty("running")]
        public DateTime? Running { get; set; }

        [JsonProperty("Retrieved")]
        public float? Retrieved { get; set; }

        [JsonProperty("completed")]
        public float? Completed { get; set; }

        [JsonProperty("failed")]
        public float? Failed { get; set; }

        [JsonProperty("failedReason")]
        public string FailedReason { get; set; }

        [JsonProperty("documents")]
        public List<CmsDocumentEntity> CmsDocuments { get; set; } = new List<CmsDocumentEntity>();

        [JsonProperty("pcdRequests")]
        public List<PcdRequestEntity> PcdRequests { get; set; } = new List<PcdRequestEntity>();

        [JsonProperty("defendantsAndCharges")]
        public DefendantsAndChargesEntity DefendantsAndCharges { get; set; } = null; // null is the default state (do not initialise to an empty object)

        public Task<DateTime> GetStartTime()
        {
            return Task.FromResult(Running.GetValueOrDefault());
        }

        public void Reset()
        {
            Status = CaseRefreshStatus.NotStarted;
            Running = null;
            Retrieved = null;
            Completed = null;
            Failed = null;
            FailedReason = null;
        }

        public async Task<CaseDeltasEntity> GetCaseDocumentChanges((CmsDocumentDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args)
        {
            var (cmsDocuments, pcdRequests, defendantsAndCharges) = args;

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(cmsDocuments.ToList());
            var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(pcdRequests.ToList());
            var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = GetDeltaDefendantsAndCharges(defendantsAndCharges);

            var deltas = new CaseDeltasEntity
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

            return await Task.FromResult(deltas);
        }

        private (List<CmsDocumentDto>, List<CmsDocumentDto>, List<long>) GetDeltaCmsDocuments(List<CmsDocumentDto> incomingDocuments)
        {
            var newDocuments =
                (from incomingDocument in incomingDocuments
                 let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
                 where cmsDocument == null
                 select incomingDocument).ToList();

            var updatedDocuments =
                (from incomingDocument in incomingDocuments
                 let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
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
                         cmsDocument.Reference != incomingDocument.Reference
                     )
                 )
                 select incomingDocument).ToList();

            // todo: filetype check here

            var deletedCmsDocumentIdsToRemove
                = CmsDocuments.Where(doc => !incomingDocuments.Any(incomingDoc => incomingDoc.DocumentId == doc.CmsDocumentId))
                    .Select(doc => doc.CmsDocumentId)
                    .ToList();

            return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
        }

        private (List<PcdRequestCoreDto> createdPcdRequests, List<PcdRequestCoreDto> updatedPcdRequests, List<long> deletedPcdRequests) GetDeltaPcdRequests(List<PcdRequestCoreDto> incomingPcdRequests)
        {
            var newPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => !PcdRequests.Any(pcd => pcd.CmsDocumentId == incomingPcd.Id))
                    .ToList();

            // Return empty list for updated pcds.  Before this we had the following:
            //     .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != DocumentStatus.Indexed))
            // which did nothing.  We could be doing some sort of hash comparison here on pcd requests to see if they've changed, but this would
            // involve always having to request the full pcd request from DDEI on every refresh, which would be costly.
            var updatedPcdRequests = Enumerable.Empty<PcdRequestCoreDto>().ToList();

            var deletedPcdRequestIds
                = PcdRequests.Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.CmsDocumentId))
                    .Select(pcd => pcd.CmsDocumentId)
                    .ToList();

            return (newPcdRequests, updatedPcdRequests, deletedPcdRequestIds);
        }

        private (DefendantsAndChargesListDto createdDefendantsAndCharges, DefendantsAndChargesListDto updatedDefendantsAndCharges, bool deletedDefendantsAndCharges) GetDeltaDefendantsAndCharges(DefendantsAndChargesListDto incomingDefendantsAndCharges)
        {
            DefendantsAndChargesListDto newDefendantsAndCharges = null, updatedDefendantsAndCharges = null;

            if (DefendantsAndCharges == null && incomingDefendantsAndCharges != null)
                newDefendantsAndCharges = incomingDefendantsAndCharges;

            if (DefendantsAndCharges != null && incomingDefendantsAndCharges != null)
            {
                if (DefendantsAndCharges.VersionId != incomingDefendantsAndCharges.VersionId)
                    updatedDefendantsAndCharges = incomingDefendantsAndCharges;
            }

            var deletedDefendantsAndCharges = DefendantsAndCharges != null && incomingDefendantsAndCharges == null;

            return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
        }

        private List<(CmsDocumentEntity, DocumentDeltaType)> CreateTrackerCmsDocuments(List<CmsDocumentDto> createdDocuments)
        {
            var newDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument
                    = new CmsDocumentEntity
                    (
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

                CmsDocuments.Add(trackerDocument);
                newDocuments.Add((trackerDocument, DocumentDeltaType.RequiresIndexing));
            }

            return newDocuments;
        }

        private List<(CmsDocumentEntity, DocumentDeltaType)> UpdateTrackerCmsDocuments(List<CmsDocumentDto> updatedDocuments)
        {
            var changedDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>();

            foreach (var updatedDocument in updatedDocuments)
            {
                var trackerDocument = CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

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

                changedDocuments.Add((trackerDocument, caseDeltaType));
            }

            return changedDocuments
                .Where(d => d.Item2 != DocumentDeltaType.DoesNotRequireRefresh)
                .ToList();
        }

        private List<CmsDocumentEntity> DeleteTrackerCmsDocuments(List<long> documentIdsToDelete)
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

        private List<PcdRequestEntity> CreateTrackerPcdRequests(List<PcdRequestCoreDto> createdPcdRequests)
        {
            var newPcdRequests = new List<PcdRequestEntity>();

            foreach (var newPcdRequest in createdPcdRequests)
            {

                var trackerPcdRequest = new PcdRequestEntity(newPcdRequest.Id, 1, newPcdRequest);
                PcdRequests.Add(trackerPcdRequest);
                newPcdRequests.Add(trackerPcdRequest);
            }

            return newPcdRequests;
        }

        private List<PcdRequestEntity> UpdateTrackerPcdRequests(List<PcdRequestCoreDto> updatedPcdRequests)
        {
            var changedPcdRequests = new List<PcdRequestEntity>();

            foreach (var updatedPcdRequest in updatedPcdRequests)
            {
                var trackerPcdRequest = PcdRequests.Find(pcd => pcd.CmsDocumentId == updatedPcdRequest.Id);
                trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

                changedPcdRequests.Add(trackerPcdRequest);
            }

            return changedPcdRequests;
        }

        private List<PcdRequestEntity> DeleteTrackerPcdRequests(List<long> deletedPcdRequestIds)
        {
            var deletePcdRequests
                = PcdRequests
                    .Where(pcd => deletedPcdRequestIds.Contains(pcd.CmsDocumentId))
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

                DefendantsAndCharges = new DefendantsAndChargesEntity(
                    createdDefendantsAndCharges.CaseId,
                    createdDefendantsAndCharges.VersionId,
                    createdDefendantsAndCharges);

                return DefendantsAndCharges;
            }

            return null;
        }

        private DefendantsAndChargesEntity UpdateTrackerDefendantsAndCharges(DefendantsAndChargesListDto updatedDefendantsAndCharges)
        {
            if (updatedDefendantsAndCharges != null)
            {
                // todo: encapsulate this logic into DefendantsAndChargesEntity
                DefendantsAndCharges.HasMultipleDefendants = updatedDefendantsAndCharges?.DefendantsAndCharges.Count() > 1;
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

        private BaseDocumentEntity GetDocument(string documentId)
        {
            var cmsDocument = CmsDocuments.Find(doc => doc.DocumentId == documentId);
            if (cmsDocument != null)
            {
                return cmsDocument;
            }

            var pcdRequest = PcdRequests.Find(pcd => pcd.DocumentId == documentId);
            if (pcdRequest != null)
            {
                return pcdRequest;
            }

            if (DefendantsAndCharges != null)
                return DefendantsAndCharges;

            return null;
        }

        public void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args)
        {
            var (t, status, info) = args;

            Status = status;

            switch (status)
            {
                case CaseRefreshStatus.Running:
                    Running = t;
                    break;

                case CaseRefreshStatus.DocumentsRetrieved:
                    if (Running != null)
                        Retrieved = (float)((t - Running).Value.TotalMilliseconds / 1000.0);
                    break;

                case CaseRefreshStatus.Completed:
                    if (Running != null)
                        Completed = (float)((t - Running).Value.TotalMilliseconds / 1000.0);
                    break;

                case CaseRefreshStatus.Failed:
                    if (Running != null)
                    {
                        Failed = (float)((t - Running).Value.TotalMilliseconds / 1000.0);
                        FailedReason = info;
                    }
                    break;
            }
        }

        public void SetDocumentPdfConversionSucceeded(string documentId)
        {
            var document = GetDocument(documentId);
            document.Status = DocumentStatus.PdfUploadedToBlob;
        }

        public void SetDocumentPdfConversionFailed((string DocumentId, PdfConversionStatus PdfConversionStatus) arg)
        {
            var document = GetDocument(arg.DocumentId);
            document.Status = DocumentStatus.UnableToConvertToPdf;
            document.ConversionStatus = arg.PdfConversionStatus;
        }

        public void SetDocumentIndexingSucceeded(string documentId)
        {
            var document = GetDocument(documentId);
            document.Status = DocumentStatus.Indexed;
        }

        public void SetDocumentIndexingFailed(string documentId)
        {
            var document = GetDocument(documentId);
            document.Status = DocumentStatus.OcrAndIndexFailure;
        }

        public void SetPiiVersionId(string documentId)
        {
            var document = GetDocument(documentId);

            document.PiiVersionId = document.VersionId;
        }
    }
}