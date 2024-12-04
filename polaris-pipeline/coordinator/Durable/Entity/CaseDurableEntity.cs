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
using System.Text.Json.Serialization;
using Microsoft.DurableTask.Entities;
using coordinator.Domain;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class CaseDurableEntity : ICaseDurableEntity
    {
        public static string GetKey(int caseId) => $"[{caseId}]";

        public static EntityInstanceId GetEntityId(int caseId) => new (nameof(CaseDurableEntity), GetKey(caseId));

        [Function(nameof(CaseDurableEntity))]
        public static Task Run([EntityTrigger] TaskEntityDispatcher taskEntityDispatcher)
        {
            return taskEntityDispatcher.DispatchAsync<CaseDurableEntity>();
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        [JsonInclude]
        public CaseRefreshStatus Status { get; set; }

        [JsonPropertyName("running")]
        [JsonInclude]
        public DateTime? Running { get; set; }

        [JsonPropertyName("Retrieved")]
        [JsonInclude]
        public float? Retrieved { get; set; }

        [JsonPropertyName("completed")]
        [JsonInclude]
        public float? Completed { get; set; }

        [JsonPropertyName("failed")]
        [JsonInclude]
        public float? Failed { get; set; }

        [JsonPropertyName("failedReason")]
        [JsonInclude]
        public string FailedReason { get; set; }

        [JsonPropertyName("documents")]
        [JsonInclude]
        public List<CmsDocumentEntity> CmsDocuments { get; set; } = [];

        [JsonPropertyName("pcdRequests")]
        [JsonInclude]
        public List<PcdRequestEntity> PcdRequests { get; set; } = [];

        [JsonPropertyName("defendantsAndCharges")]
        [JsonInclude]
        public DefendantsAndChargesEntity DefendantsAndCharges { get; set; } = null; // null is the default state (do not initialise to an empty object)

        public Task<DateTime> GetStartTime() => Task.FromResult(Running.GetValueOrDefault());

        public void Reset()
        {
            Status = CaseRefreshStatus.NotStarted;
            Running = null;
            Retrieved = null;
            Completed = null;
            Failed = null;
            FailedReason = null;
        }

        public async Task<CaseDeltasEntity> GetCaseDocumentChanges(GetCaseDocumentsResponse getCaseDocumentsResponse)
        {
            var cmsDocuments = getCaseDocumentsResponse.CmsDocuments;
            var pcdRequests = getCaseDocumentsResponse.PcdRequests;
            var defendantsAndCharges = getCaseDocumentsResponse.DefendantAndCharges;

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments([.. cmsDocuments]);
            var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests([.. pcdRequests]);
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
            {
                newDefendantsAndCharges = incomingDefendantsAndCharges;
            }

            if (DefendantsAndCharges != null && incomingDefendantsAndCharges != null)
            {
                if (DefendantsAndCharges.VersionId != incomingDefendantsAndCharges.VersionId)
                {
                    updatedDefendantsAndCharges = incomingDefendantsAndCharges;
                }
            }

            var deletedDefendantsAndCharges = DefendantsAndCharges != null && incomingDefendantsAndCharges == null;

            return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
        }

        private List<DocumentDelta> CreateTrackerCmsDocuments(List<CmsDocumentDto> createdDocuments)
        {
            var newDocuments = new List<DocumentDelta>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument = new CmsDocumentEntity
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
                newDocuments.Add(new DocumentDelta { Document = trackerDocument, DeltaType = DocumentDeltaType.RequiresIndexing });
            }

            return newDocuments;
        }

        private List<DocumentDelta> UpdateTrackerCmsDocuments(List<CmsDocumentDto> updatedDocuments)
        {
            var changedDocuments = new List<DocumentDelta>();

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

                changedDocuments.Add(new DocumentDelta { Document = trackerDocument, DeltaType = caseDeltaType });
            }

            return changedDocuments
                .Where(d => d.DeltaType != DocumentDeltaType.DoesNotRequireRefresh)
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
            {
                return DefendantsAndCharges;
            }

            return null;
        }

        public void SetCaseStatus(SetCaseStatusPayload payload)
        {
            Status = payload.Status;

            switch (Status)
            {
                case CaseRefreshStatus.Running:
                    Running = payload.UpdatedAt;
                    break;

                case CaseRefreshStatus.DocumentsRetrieved:
                    if (Running != null)
                    {
                        Retrieved = (float)((payload.UpdatedAt - Running).Value.TotalMilliseconds / 1000.0);
                    }

                    break;

                case CaseRefreshStatus.Completed:
                    if (Running != null)
                    {
                        Completed = (float)((payload.UpdatedAt - Running).Value.TotalMilliseconds / 1000.0);
                    }

                    break;

                case CaseRefreshStatus.Failed:
                    if (Running != null)
                    {
                        Failed = (float)((payload.UpdatedAt - Running).Value.TotalMilliseconds / 1000.0);
                        FailedReason = payload.FailedReason;
                    }
                    break;
            }
        }

        public void SetDocumentPdfConversionSucceeded(string documentId)
        {
            var document = GetDocument(documentId);
            document.Status = DocumentStatus.PdfUploadedToBlob;
        }

        public void SetDocumentPdfConversionFailed(SetDocumentPdfConversionFailedPayload payload)
        {
            var document = GetDocument(payload.DocumentId);
            document.Status = DocumentStatus.UnableToConvertToPdf;
            document.ConversionStatus = payload.PdfConversionStatus;
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