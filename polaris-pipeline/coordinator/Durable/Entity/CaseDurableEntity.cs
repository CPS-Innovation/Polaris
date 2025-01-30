using Common.Dto.Response.Case;
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
            Completed = null;
            Failed = null;
            FailedReason = null;
        }

        public async Task<CaseDeltasEntity> GetCaseDocumentChanges((CmsDocumentCoreDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListCoreDto DefendantsAndCharges) args)
        {
            var (cmsDocuments, pcdRequests, defendantsAndCharges) = args;

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(cmsDocuments.ToList());
            var (createdPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(pcdRequests.ToList());
            var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = GetDeltaDefendantsAndCharges(defendantsAndCharges);

            var deltas = new CaseDeltasEntity
            {
                CreatedCmsDocuments = CreateTrackerCmsDocuments(createdDocuments),
                UpdatedCmsDocuments = UpdateTrackerCmsDocuments(updatedDocuments),
                DeletedCmsDocuments = DeleteTrackerCmsDocuments(deletedDocuments),
                CreatedPcdRequests = CreateTrackerPcdRequests(createdPcdRequests),
                DeletedPcdRequests = DeleteTrackerPcdRequests(deletedPcdRequests),
                CreatedDefendantsAndCharges = CreateTrackerDefendantsAndCharges(createdDefendantsAndCharges),
                UpdatedDefendantsAndCharges = UpdateTrackerDefendantsAndCharges(updatedDefendantsAndCharges),
                IsDeletedDefendantsAndCharges = DeleteTrackerDefendantsAndCharges(deletedDefendantsAndCharges),
            };

            return await Task.FromResult(deltas);
        }

        private (List<CmsDocumentCoreDto>, List<CmsDocumentCoreDto>, List<long>) GetDeltaCmsDocuments(List<CmsDocumentCoreDto> incomingDocuments)
        {
            var newDocuments =
                (from incomingDocument in incomingDocuments
                 let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
                 where cmsDocument == null
                 select incomingDocument).ToList();

            var updatedDocuments =
                (from incomingDocument in incomingDocuments
                 let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
                 where cmsDocument != null && cmsDocument.VersionId != incomingDocument.VersionId

                 select incomingDocument).ToList();

            // todo: filetype check here

            var deletedCmsDocumentIdsToRemove
                = CmsDocuments.Where(doc => !incomingDocuments.Any(incomingDoc => incomingDoc.DocumentId == doc.CmsDocumentId))
                    .Select(doc => doc.CmsDocumentId)
                    .ToList();

            return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
        }

        private (List<PcdRequestCoreDto> createdPcdRequests, List<long> deletedPcdRequests) GetDeltaPcdRequests(List<PcdRequestCoreDto> incomingPcdRequests)
        {
            var newPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => !PcdRequests.Any(pcd => pcd.CmsDocumentId == incomingPcd.Id))
                    .ToList();

            var deletedPcdRequestIds
                = PcdRequests.Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.CmsDocumentId))
                    .Select(pcd => pcd.CmsDocumentId)
                    .ToList();

            return (newPcdRequests, deletedPcdRequestIds);
        }

        private (DefendantsAndChargesListCoreDto createdDefendantsAndCharges, DefendantsAndChargesListCoreDto updatedDefendantsAndCharges, bool deletedDefendantsAndCharges) GetDeltaDefendantsAndCharges(DefendantsAndChargesListCoreDto incomingDefendantsAndCharges)
        {
            DefendantsAndChargesListCoreDto newDefendantsAndCharges = null, updatedDefendantsAndCharges = null;

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

        private List<(CmsDocumentEntity, DocumentDeltaType)> CreateTrackerCmsDocuments(List<CmsDocumentCoreDto> createdDocuments)
        {
            var newDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument
                    = new CmsDocumentEntity
                    (
                        cmsDocumentId: newDocument.DocumentId,
                        versionId: newDocument.VersionId,
                        path: newDocument.Path);

                CmsDocuments.Add(trackerDocument);
                newDocuments.Add((trackerDocument, DocumentDeltaType.RequiresIndexing));
            }

            return newDocuments;
        }

        private List<(CmsDocumentEntity, DocumentDeltaType)> UpdateTrackerCmsDocuments(List<CmsDocumentCoreDto> updatedDocuments)
        {
            var changedDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>();

            foreach (var updatedDocument in updatedDocuments)
            {
                var trackerDocument = CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);
                trackerDocument.VersionId = updatedDocument.VersionId;
                trackerDocument.Path = updatedDocument.Path;
                var caseDeltaType = DocumentDeltaType.RequiresIndexing;

                changedDocuments.Add((trackerDocument, caseDeltaType));
            }

            return changedDocuments;
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

                var trackerPcdRequest = new PcdRequestEntity(newPcdRequest.Id, 1);
                PcdRequests.Add(trackerPcdRequest);
                newPcdRequests.Add(trackerPcdRequest);
            }

            return newPcdRequests;
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

        private DefendantsAndChargesEntity CreateTrackerDefendantsAndCharges(DefendantsAndChargesListCoreDto createdDefendantsAndCharges)
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

        private DefendantsAndChargesEntity UpdateTrackerDefendantsAndCharges(DefendantsAndChargesListCoreDto updatedDefendantsAndCharges)
        {
            if (updatedDefendantsAndCharges != null)
            {
                // todo: encapsulate this logic into DefendantsAndChargesEntity
                DefendantsAndCharges.HasMultipleDefendants = updatedDefendantsAndCharges.DefendantCount > 1;
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
    }
}