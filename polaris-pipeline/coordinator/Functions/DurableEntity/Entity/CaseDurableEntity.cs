using Common.Domain.Entity;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.ValueObjects;
using coordinator.Functions.Orchestration;
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
        public static string GetInstanceId(string caseId)
        {
            var caseEntityId = new EntityId(
                nameof(CaseDurableEntity),
                RefreshCaseOrchestrator.GetKey(caseId.ToString())
            );
            return caseEntityId.ToString();
        }

        public static ICaseDurableEntity GetCaseDurableEntity(IDurableOrchestrationContext context, long caseId)
        {
            var entityId = GetInstanceId(caseId.ToString());
            return context.CreateEntityProxy<ICaseDurableEntity>(entityId);
        }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        private int? version = null;

        [JsonProperty("versionId")]
        public int? Version
        {
            get { return version; }
            set { version = value; }
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
        public List<CmsDocumentEntity> CmsDocuments { get; set; }

        [JsonProperty("pcdRequests")]
        public List<PcdRequestEntity> PcdRequests { get; set; }

        [JsonProperty("defendantsAndCharges")]
        public DefendantsAndChargesEntity DefendantsAndCharges { get; set; }

        public void Reset(string transactionId)
        {
            TransactionId = transactionId;
            Status = CaseRefreshStatus.NotStarted;
            Running = null;
            Retrieved = null;
            Completed = null;
            Failed = null;
            FailedReason = null;
            CmsDocuments = CmsDocuments ?? new List<CmsDocumentEntity>();
            PcdRequests = PcdRequests ?? new List<PcdRequestEntity>();
            DefendantsAndCharges = DefendantsAndCharges ?? null;
        }

        public async Task<CaseDeltasEntity> GetCaseDocumentChanges((CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args)
        {
            var (cmsDocuments, pcdRequests, defendantsAndCharges) = args;

            CmsDocuments = CmsDocuments ?? new List<CmsDocumentEntity>();
            PcdRequests = PcdRequests ?? new List<PcdRequestEntity>();
            DefendantsAndCharges = DefendantsAndCharges ?? null;

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

        public Task<bool> AllDocumentsFailed()
        {
            var statuses =
                CmsDocuments
                    .Select(doc => doc.Status)
                    .Concat(PcdRequests.Select(pcd => pcd.Status))
                    .Append(DefendantsAndCharges.Status)
                    .ToList();

            return Task.FromResult(
                statuses.All(s => s is DocumentStatus.UnableToConvertToPdf));
        }

        private (List<CmsDocumentDto>, List<CmsDocumentDto>, List<string>) GetDeltaCmsDocuments(List<CmsDocumentDto> incomingDocuments)
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
                         cmsDocument.Status != DocumentStatus.Indexed ||
                         cmsDocument.CmsVersionId != incomingDocument.VersionId ||
                         cmsDocument.IsDispatched != incomingDocument.IsDispatched ||
                         cmsDocument.IsOcrProcessed != incomingDocument.IsOcrProcessed ||
                         cmsDocument.CmsDocType?.DocumentTypeId != incomingDocument.CmsDocType?.DocumentTypeId
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
                    .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != DocumentStatus.Indexed))
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

            var deletedDefendantsAndCharges = DefendantsAndCharges != null && incomingDefendantsAndCharges == null;

            return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
        }

        private List<CmsDocumentEntity> CreateTrackerCmsDocuments(List<CmsDocumentDto> createdDocuments)
        {
            var newDocuments = new List<CmsDocumentEntity>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument
                    = new CmsDocumentEntity
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
                        presentationFlags: newDocument.PresentationFlags
                    );

                CmsDocuments.Add(trackerDocument);
                newDocuments.Add(trackerDocument);
            }

            return newDocuments;
        }

        private List<CmsDocumentEntity> UpdateTrackerCmsDocuments(List<CmsDocumentDto> updatedDocuments)
        {
            var changedDocuments = new List<CmsDocumentEntity>();

            foreach (var updatedDocument in updatedDocuments)
            {
                var trackerDocument = CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

                trackerDocument.PolarisDocumentVersionId++;
                trackerDocument.CmsVersionId = updatedDocument.VersionId;
                trackerDocument.CmsDocType = updatedDocument.CmsDocType;
                trackerDocument.Path = updatedDocument.Path;
                trackerDocument.CmsOriginalFileExtension = updatedDocument.FileExtension;
                trackerDocument.CmsFileCreatedDate = updatedDocument.DocumentDate;
                trackerDocument.PresentationTitle = updatedDocument.PresentationTitle;
                trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;
                trackerDocument.IsOcrProcessed = updatedDocument.IsOcrProcessed;
                trackerDocument.IsDispatched = updatedDocument.IsDispatched;
                trackerDocument.CmsParentDocumentId = updatedDocument.ParentDocumentId;
                trackerDocument.WitnessId = updatedDocument.WitnessId;
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

        private BaseDocumentEntity GetDocument(PolarisDocumentId polarisDocumentId)
        {
            var cmsDocument = CmsDocuments.Find(doc => doc.PolarisDocumentId.ToString().Equals(polarisDocumentId.ToString(), StringComparison.OrdinalIgnoreCase));
            if (cmsDocument != null)
            {
                return cmsDocument;
            }

            var pcdRequest = PcdRequests.Find(pcd => pcd.PolarisDocumentId.ToString().Equals(polarisDocumentId.ToString(), StringComparison.OrdinalIgnoreCase));
            if (pcdRequest != null)
            {
                return pcdRequest;
            }

            return DefendantsAndCharges;
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

        public void SetDocumentStatus((PolarisDocumentId PolarisDocumentId, DocumentStatus Status) args)
        {
            var (polarisDocumentId, status) = args;
            var document = GetDocument(polarisDocumentId);

            document.Status = status;
        }

        public void SetDocumentPdfBlobName((PolarisDocumentId PolarisDocumentId, string PdfBlobName) args)
        {
            var (polarisDocumentId, pdfBlobName) = args;
            var document = GetDocument(polarisDocumentId);

            document.IsPdfAvailable = true;
            document.PdfBlobName = pdfBlobName;
        }

        public Task<DateTime> GetStartTime()
        {
            return Task.FromResult(Running.GetValueOrDefault());
        }

        [FunctionName(nameof(CaseDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseDurableEntity>();
        }
    }
}