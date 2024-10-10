using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Durable.Orchestration;
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
using Common.Domain.Document;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    [JsonObject(MemberSerialization.OptIn)]
    public class CaseDurableEntity : ICaseDurableEntity
    {
        public static string GetInstanceId(string caseId)
        {
            return $"@{nameof(CaseDurableEntity).ToLower()}@{RefreshCaseOrchestrator.GetKey(caseId)}";
        }

        [Obsolete]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }


        private int? version = null;

        // Currently useful in analytics to e.g. determine if/when a case has been refreshed
        [JsonProperty("versionId")]
        public int? Version
        {
            get { return version; }
            set { version = value; }
        }

        [Obsolete]
        public Task<int?> GetVersion()
        {
            return Task.FromResult(version);
        }

        [Obsolete]
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

        [Obsolete]
        public void Reset(string transactionId)
        {
            TransactionId = transactionId;
            Status = CaseRefreshStatus.NotStarted;
            Running = null;
            Retrieved = null;
            Completed = null;
            Failed = null;
            FailedReason = null;
            // todo: this initialisation should be done in a more constructor-like way, at least only once
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
                         cmsDocument.CmsVersionId != incomingDocument.VersionId ||
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

        private (List<PcdRequestDto> createdPcdRequests, List<PcdRequestDto> updatedPcdRequests, List<int> deletedPcdRequests) GetDeltaPcdRequests(List<PcdRequestDto> incomingPcdRequests)
        {
            var newPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => !PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id))
                    .ToList();

            // Return empty list for updated pcds.  Before this we had the following:
            //     .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != DocumentStatus.Indexed))
            // which did nothing.  We could be doing some sort of hash comparison here on pcd requests to see if they've changed, but this would
            // involve always having to request the full pcd request from DDEI on every refresh, which would be costly.
            var updatedPcdRequests = Enumerable.Empty<PcdRequestDto>().ToList();

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

        private List<(CmsDocumentEntity, DocumentDeltaType)> CreateTrackerCmsDocuments(List<CmsDocumentDto> createdDocuments)
        {
            var newDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument
                    = new CmsDocumentEntity
                    (
                        polarisDocumentId: PolarisDocumentIdHelper.GetPolarisDocumentId(PolarisDocumentType.CmsDocument, newDocument.DocumentId),
                        cmsDocumentId: newDocument.DocumentId,
                        cmsVersionId: newDocument.VersionId,
                        cmsDocType: newDocument.CmsDocType,
                        path: newDocument.Path,
                        cmsFileCreatedDate: newDocument.DocumentDate,
                        cmsOriginalFileName: newDocument.FileName,
                        presentationTitle: newDocument.PresentationTitle,
                        isOcrProcessed: newDocument.IsOcrProcessed,
                        isDispatched: newDocument.IsDispatched,
                        categoryListOrder: newDocument.CategoryListOrder,
                        polarisParentDocumentId: PolarisDocumentIdHelper.GetPolarisDocumentId(PolarisDocumentType.CmsDocument, newDocument.ParentDocumentId),
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

                if (trackerDocument.CmsVersionId != updatedDocument.VersionId)
                {
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
                var polarisDocumentId = PolarisDocumentIdHelper.GetPolarisDocumentId(PolarisDocumentType.PcdRequest, newPcdRequest.Id.ToString());
                var trackerPcdRequest = new PcdRequestEntity(polarisDocumentId, newPcdRequest);
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
                var documentId = PolarisDocumentIdHelper.GetPolarisDocumentId(PolarisDocumentType.DefendantsAndCharges, createdDefendantsAndCharges.CaseId.ToString());
                DefendantsAndCharges = new DefendantsAndChargesEntity(documentId, createdDefendantsAndCharges);

                return DefendantsAndCharges;
            }

            return null;
        }

        private DefendantsAndChargesEntity UpdateTrackerDefendantsAndCharges(DefendantsAndChargesListDto updatedDefendantsAndCharges)
        {
            if (updatedDefendantsAndCharges != null)
            {
                DefendantsAndCharges.DefendantsAndCharges = updatedDefendantsAndCharges;
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

        private BaseDocumentEntity GetDocument(string polarisDocumentId)
        {
            var cmsDocument = CmsDocuments.Find(doc => doc.PolarisDocumentId == polarisDocumentId);
            if (cmsDocument != null)
            {
                return cmsDocument;
            }

            var pcdRequest = PcdRequests.Find(pcd => pcd.PolarisDocumentId == polarisDocumentId);
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

        [Obsolete]
        public Task<string[]> GetPolarisDocumentIds()
        {
            var polarisDocumentIds =
                CmsDocuments?.Select(doc => doc.PolarisDocumentId.ToString())
                    .Union(PcdRequests?.Select(pcd => pcd.PolarisDocumentId.ToString())
                    .Union(new string[] { DefendantsAndCharges?.PolarisDocumentId.ToString() }))
                    .ToArray();

            return Task.FromResult(polarisDocumentIds);
        }

        [Obsolete]
        public void SetDocumentFlags((string PolarisDocumentId, bool IsOcrProcessed, bool IsDispatched) args)
        {
            var (polarisDocumentId, isOcrProcessed, isDispatched) = args;

            var document = GetDocument(polarisDocumentId) as CmsDocumentEntity;
            document.IsOcrProcessed = isOcrProcessed;
            document.IsDispatched = isDispatched;
        }

        public void SetDocumentStatus((string PolarisDocumentId, DocumentStatus Status, string PdfBlobName) args)
        {
            var (polarisDocumentId, status, pdfBlobName) = args;

            var document = GetDocument(polarisDocumentId);
            document.Status = status;

            if (status == DocumentStatus.PdfUploadedToBlob)
            {
                document.PdfBlobName = pdfBlobName;
            }
        }

        public void SetDocumentConversionStatus((string PolarisDocumentId, PdfConversionStatus Status) args)
        {
            var (polarisDocumentId, status) = args;

            var document = GetDocument(polarisDocumentId);
            document.ConversionStatus = status;
        }

        // Only required when debugging to manually set the Tracker state
        [Obsolete]
        public void SetValue(CaseDurableEntity tracker)
        {
            Status = tracker.Status;
            Running = tracker.Running;
            Retrieved = tracker.Retrieved;
            Completed = tracker.Completed;
            Failed = tracker.Failed;
            FailedReason = tracker.FailedReason;
            CmsDocuments = tracker.CmsDocuments;
            PcdRequests = tracker.PcdRequests;
            DefendantsAndCharges = tracker.DefendantsAndCharges;
        }

        public Task<DateTime> GetStartTime()
        {
            return Task.FromResult(Running.GetValueOrDefault());
        }

        [Obsolete]
        public Task<float> GetDurationToCompleted()
        {
            return Task.FromResult(Completed.GetValueOrDefault());
        }

        [FunctionName(nameof(CaseDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseDurableEntity>();
        }

        public void SetDocumentPdfConversionSucceeded((string polarisDocumentId, string pdfBlobName) arg)
        {
            var document = GetDocument(arg.polarisDocumentId);
            document.PdfBlobName = arg.pdfBlobName;
            document.Status = DocumentStatus.PdfUploadedToBlob;
        }

        public void SetDocumentPdfConversionFailed((string PolarisDocumentId, PdfConversionStatus PdfConversionStatus) arg)
        {
            var document = GetDocument(arg.PolarisDocumentId);
            document.Status = DocumentStatus.UnableToConvertToPdf;
            document.ConversionStatus = arg.PdfConversionStatus;
        }

        public void SetDocumentIndexingSucceeded(string polarisDocumentId)
        {
            var document = GetDocument(polarisDocumentId);
            document.Status = DocumentStatus.Indexed;
        }

        public void SetDocumentIndexingFailed(string polarisDocumentId)
        {
            var document = GetDocument(polarisDocumentId);
            document.Status = DocumentStatus.OcrAndIndexFailure;
        }

        public void SetPiiCmsVersionId(string polarisDocumentId)
        {
            var document = GetDocument(polarisDocumentId);

            document.PiiCmsVersionId = document.CmsVersionId;
        }
    }
}