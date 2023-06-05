using Common.Domain.Entity;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.ValueObjects;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    [JsonObject(MemberSerialization.OptIn)]
    public class CaseDurableEntity : ICaseDurableEntity
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        private int? version = null;

        [JsonProperty("versionId")]
        public int? Version 
        { 
            get { return version; } 
            set { version = value;  } 
        }

        public Task<int?> GetVersion()
        {
            return Task.FromResult(version);
        }

        public void SetVersion(int value)
        {
            version = value;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        public List<CmsDocumentEntity> CmsDocuments { get; set; }

        [JsonProperty("pcdRequests")]
        public List<PcdRequestEntity> PcdRequests { get; set; }

        [JsonProperty("defendantsAndCharges")]
        public DefendantsAndChargesEntity DefendantsAndCharges { get; set; }

        public void Reset(string transactionId)
        {
            TransactionId = transactionId;
            ProcessingCompleted = null;
            Status = TrackerStatus.Running;
            CmsDocuments = CmsDocuments ?? new List<CmsDocumentEntity>();
            PcdRequests = PcdRequests ?? new List<PcdRequestEntity>();
            DefendantsAndCharges = DefendantsAndCharges ?? null;
        }

        public async Task<CaseDeltasEntity> GetCaseDocumentChanges((DateTime t, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args)
        {
            var (t, cmsDocuments, pcdRequests, defendantsAndCharges) = args;

            CmsDocuments = CmsDocuments ?? new List<CmsDocumentEntity>();
            PcdRequests = PcdRequests ?? new List<PcdRequestEntity>();
            DefendantsAndCharges = DefendantsAndCharges ?? null;

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(cmsDocuments.ToList());
            var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(pcdRequests.ToList());
            var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = GetDeltaDefendantsAndCharges(defendantsAndCharges);

            CaseDeltasEntity deltas = new CaseDeltasEntity
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

            Status = TrackerStatus.DocumentsRetrieved;
            DocumentsRetrieved = t;

            return await Task.FromResult(deltas);
        }

        public void RegisterDocumentStatus((string PolarisDocumentId, TrackerDocumentStatus Status, string PdfBlobName) args)
        {
            var (polarisDocumentId, status, pdfBlobName) = args;

            var document = GetBaseTracker(polarisDocumentId);
            document.Status = status;

            if (status == TrackerDocumentStatus.PdfUploadedToBlob)
            {
                document.IsPdfAvailable = true;
            }
            if (status == TrackerDocumentStatus.PdfUploadedToBlob || status == TrackerDocumentStatus.DocumentAlreadyProcessed)
            {
                document.PdfBlobName = pdfBlobName;
            }
        }

        public void RegisterCompleted((DateTime T, bool Success) args)
        {
            var (t, success) = args;

            Status = success ? TrackerStatus.Completed : TrackerStatus.Failed;
            ProcessingCompleted = t;
        }

        public Task<bool> AllDocumentsFailed()
        {
            var statuses =
                CmsDocuments
                    .Select(doc => doc.Status)
                    .Concat(PcdRequests.Select(pcd => pcd.Status))
                    .Append(DefendantsAndCharges.Status)
                    .ToList();

            return Task.FromResult(
                statuses.All(s => s is TrackerDocumentStatus.UnableToConvertToPdf or TrackerDocumentStatus.UnexpectedFailure));
        }

        // Only required when debugging to manually set the Tracker state
        public void SetValue(CaseDurableEntity tracker)
        {
            Status = tracker.Status;
            ProcessingCompleted = tracker.ProcessingCompleted;
            CmsDocuments = tracker.CmsDocuments;
            PcdRequests = tracker.PcdRequests;
            DefendantsAndCharges = tracker.DefendantsAndCharges;
        }

        private (List<DocumentDto>, List<DocumentDto>, List<string>) GetDeltaCmsDocuments(List<DocumentDto> incomingDocuments)
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
                        cmsDocument.Status != TrackerDocumentStatus.Indexed || 
                        cmsDocument.CmsVersionId != incomingDocument.VersionId ||
                        cmsDocument.IsOcrProcessed != incomingDocument.IsOcrProcessed 
                    )
                )
                select incomingDocument).ToList();

            var deletedCmsDocumentIdsToRemove
                = CmsDocuments.Where(doc => !incomingDocuments.Any(incomingDoc => incomingDoc.DocumentId == doc.CmsDocumentId))
                    .Select(doc => doc.CmsDocumentId)
                    .ToList();

            return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
        }

        private (List<PcdRequestDto> createdPcdRequests, List<PcdRequestDto> updatedPcdRequests, List<int> deletedPcdRequests) GetDeltaPcdRequests(List<PcdRequestDto> incomingPcdRequests)
        {
            var newPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => !PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id))
                    .ToList();

            var updatedPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != TrackerDocumentStatus.Indexed))
                    .ToList();

            var deletedPcdRequestIds
                = PcdRequests.Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.PcdRequest.Id))
                    .Select(pcd => pcd.PcdRequest.Id)
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
                if (JsonConvert.SerializeObject(DefendantsAndCharges.DefendantsAndCharges) != JsonConvert.SerializeObject(incomingDefendantsAndCharges))
                    updatedDefendantsAndCharges = incomingDefendantsAndCharges;
            }

            var deletedDefendantsAndCharges = (DefendantsAndCharges != null && incomingDefendantsAndCharges == null);

            return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
        }

        private List<CmsDocumentEntity> CreateTrackerCmsDocuments(List<DocumentDto> createdDocuments)
        {
            var newDocuments = new List<CmsDocumentEntity>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument 
                    = new CmsDocumentEntity
                    (
                        new PolarisDocumentId(PolarisDocumentType.CmsDocument, newDocument.DocumentId),
                        1,
                        newDocument.DocumentId,
                        newDocument.VersionId,
                        newDocument.CmsDocType,
                        newDocument.DocumentDate,
                        newDocument.FileName,
                        newDocument.IsOcrProcessed,
                        newDocument.PresentationFlags
                    );

                CmsDocuments.Add(trackerDocument);
                newDocuments.Add(trackerDocument);
            }

            return newDocuments;
        }

        private List<CmsDocumentEntity> UpdateTrackerCmsDocuments(List<DocumentDto> updatedDocuments)
        {
            var changedDocuments = new List<CmsDocumentEntity>();

            foreach (var updatedDocument in updatedDocuments)
            {
                var trackerDocument = CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

                trackerDocument.PolarisDocumentVersionId++;
                trackerDocument.CmsVersionId = updatedDocument.VersionId;
                trackerDocument.CmsDocType = updatedDocument.CmsDocType;
                trackerDocument.CmsFileCreatedDate = updatedDocument.DocumentDate;
                trackerDocument.CmsOriginalFileName = updatedDocument.FileName;
                trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;

                changedDocuments.Add(trackerDocument);
            }

            return changedDocuments;
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
            if(createdDefendantsAndCharges != null)
            {
                PolarisDocumentId polarisDocumentId = new PolarisDocumentId(PolarisDocumentType.DefendantsAndCharges, createdDefendantsAndCharges.CaseId.ToString());
                DefendantsAndCharges = new DefendantsAndChargesEntity(polarisDocumentId, 1, createdDefendantsAndCharges);

                return DefendantsAndCharges;
            }

            return null;
        }

        private DefendantsAndChargesEntity UpdateTrackerDefendantsAndCharges(DefendantsAndChargesListDto updatedDefendantsAndCharges)
        {
            if(updatedDefendantsAndCharges != null)
            {
                DefendantsAndCharges.DefendantsAndCharges = updatedDefendantsAndCharges;
                DefendantsAndCharges.PolarisDocumentVersionId++;

                return DefendantsAndCharges;
            }

            return null;
        }

        private bool DeleteTrackerDefendantsAndCharges(bool deletedDefendantsAndCharges)
        {
            if(deletedDefendantsAndCharges) 
            {
                DefendantsAndCharges = null;
            }

            return deletedDefendantsAndCharges;
        }

        private BaseDocumentEntity GetBaseTracker(string documentId)
        {
            var cmsDocument = CmsDocuments.Find(doc => doc.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (cmsDocument != null)
            {
                return cmsDocument;
            }

            var pcdRequest = PcdRequests.Find(pcd => pcd.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (pcdRequest != null)
            {
                return pcdRequest;
            }

            if (DefendantsAndCharges != null)
                return DefendantsAndCharges;

            return null;
        }

        private void ClearState(TrackerStatus status)
        {
            Status = status;
            CmsDocuments = new List<CmsDocumentEntity>();
            PcdRequests = new List<PcdRequestEntity>();
            DocumentsRetrieved = null;
            ProcessingCompleted = null;
        }

        [FunctionName(nameof(CaseDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseDurableEntity>();
        }
    }
}