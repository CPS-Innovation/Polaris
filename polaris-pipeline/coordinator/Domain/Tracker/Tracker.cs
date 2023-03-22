using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.DocumentEvaluation;
using Common.Domain.Pipeline;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Tracker : ITracker
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocument> Documents { get; set; }

        [JsonProperty("logs")]
        public List<Log> Logs { get; set; }

        public Task Initialise(string transactionId)
        {
            TransactionId = transactionId;

            ClearState(TrackerStatus.Running);

            Log(LogType.Initialised);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentIds(RegisterDocumentIdsArg arg)
        {
            var evaluationResults = new DocumentEvaluationActivityPayload(arg.CaseUrn, arg.CaseId, arg.CorrelationId);
            if (Documents.Count == 0) //no documents yet loaded in the tracker for this case, grab them all
            {
                Documents = arg.Documents
                    .Select(item => CreateTrackerDocument(item))
                    .ToList();
                Log(LogType.RegisteredDocumentIds);
            }
            else
            {
                //remove any documents that are no longer present in the list retrieved from CMS from the tracker so they are no reprocessed
                foreach (var trackedDocument in
                         Documents.Where(trackedDocument =>
                             !arg.Documents.Exists(x => x.DocumentId == trackedDocument.CmsDocumentId && x.VersionId == trackedDocument.CmsVersionId)))
                {
                    evaluationResults.DocumentsToRemove.Add(new DocumentToRemove(trackedDocument.CmsDocumentId, trackedDocument.CmsVersionId));
                }

                //now remove any invalid documents from the tracker so they are not reprocessed
                foreach (var item in
                         evaluationResults.DocumentsToRemove.Select(invalidDocument =>
                             Documents.Find(x => x.CmsDocumentId == invalidDocument.DocumentId && x.CmsVersionId == invalidDocument.VersionId)))
                {
                    Documents.Remove(item);
                }

                //now evaluate all incoming documents against the existing tracker record that are not already identified for removal and make sure
                //that anything new is added to the tracker
                foreach (var cmsDocument in from cmsDocument in
                             arg.Documents
                                            where !evaluationResults.DocumentsToRemove
                             .Exists(x => x.DocumentId == cmsDocument.DocumentId && x.VersionId == cmsDocument.VersionId)
                                            let item = Documents.Find(x => x.CmsDocumentId == cmsDocument.DocumentId)
                                            where item == null
                                            select cmsDocument)
                {
                    TrackerDocument trackerDocument = CreateTrackerDocument(cmsDocument);
                    Documents.Add(trackerDocument);
                    Log(LogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
                }
            }

            Status = TrackerStatus.DocumentsRetrieved;

            return Task.CompletedTask;
        }

        private TrackerDocument CreateTrackerDocument(TransitionDocument document)
        {
            return new TrackerDocument(
                document.PolarisDocumentId,
                document.DocumentId,
                document.VersionId,
                document.CmsDocType,
                document.MimeType,
                document.FileExtension,
                document.CreatedDate,
                document.OriginalFileName,
                document.PresentationFlags);
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = DocumentStatus.PdfUploadedToBlob;
                document.IsPdfAvailable = true;
            }

            Log(LogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = DocumentStatus.DocumentAlreadyProcessed;
            }

            Log(LogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.UnableToConvertToPdf;

            Log(LogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.UnexpectedFailure;

            Log(LogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterIndexed(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.Indexed;

            Log(LogType.Indexed, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.OcrAndIndexFailure;

            Log(LogType.OcrAndIndexFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted()
        {
            Status = TrackerStatus.Completed;
            Log(LogType.Completed);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterFailed()
        {
            Status = TrackerStatus.Failed;
            Log(LogType.Failed);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterDeleted()
        {
            ClearState(TrackerStatus.Deleted);
            Log(LogType.Deleted);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task<List<TrackerDocument>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task<bool> AllDocumentsFailed()
        {
            return Task.FromResult(
                Documents.All(d => d.Status is DocumentStatus.UnableToConvertToPdf or DocumentStatus.UnexpectedFailure));
        }

        public Task<bool> IsAlreadyProcessed()
        {
            return Task.FromResult(Status is TrackerStatus.Completed);
        }

        public Task<bool> IsStale(bool forceRefresh)
        {
            if (Status is TrackerStatus.Running)
                return Task.FromResult(false);

            if (forceRefresh || Status is TrackerStatus.Failed)
                return Task.FromResult(true);

            return ProcessingCompleted.HasValue
                ? Task.FromResult(ProcessingCompleted.Value.Date != DateTime.Now.Date)
                : Task.FromResult(false);
        }

        private void ClearState(TrackerStatus status)
        {
            Status = status;
            Documents = new List<TrackerDocument>();
            Logs = new List<Log>();
            ProcessingCompleted = null; //reset the processing date
        }

        private void Log(LogType status, string cmsDocumentId = null)
        {
            Logs.Add(new Log
            {
                LogType = status.ToString(),
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                CmsDocumentId = cmsDocumentId
            });
        }

        [FunctionName("Tracker")]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<Tracker>();
        }
    }
}