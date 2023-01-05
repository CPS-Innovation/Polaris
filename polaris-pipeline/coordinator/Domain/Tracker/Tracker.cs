using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Tracker : ITracker
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocument> Documents { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("logs")]
        public List<Log> Logs { get; set; }
        
        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        public Task Initialise(string transactionId)
        {
            TransactionId = transactionId;
            
            Documents ??= new List<TrackerDocument>(); //preserve last run
            foreach (var document in Documents) //but reset any document status values
                document.Status = DocumentStatus.None;
            
            Status = TrackerStatus.Running;
            Logs = new List<Log>();
            ProcessingCompleted = null; //reset the processing date

            Log(LogType.Initialised);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentIds(RegisterDocumentIdsArg arg)
        {
            var evaluationResults = new DocumentEvaluationActivityPayload(arg.CaseUrn, arg.CaseId, arg.CorrelationId);
            if (Documents.Count == 0) //no documents yet loaded in the tracker for this case, grab them all
            {
                Documents = arg.IncomingDocuments
                    .Select(item => new TrackerDocument(item.DocumentId, item.VersionId, item.OriginalFileName))
                    .ToList();
                Log(LogType.RegisteredDocumentIds);
            }
            else
            {
                //remove any documents that are no longer present in the list retrieved from CMS from the tracker so they are no reprocessed
                foreach (var trackedDocument in 
                         Documents.Where(trackedDocument => 
                             !arg.IncomingDocuments.Exists(x => x.DocumentId == trackedDocument.DocumentId && x.VersionId == trackedDocument.VersionId)))
                {
                    evaluationResults.DocumentsToRemove.Add(new DocumentToRemove(trackedDocument.DocumentId, trackedDocument.VersionId));
                }
                
                //now remove any invalid documents from the tracker so they are not reprocessed
                foreach (var item in 
                         evaluationResults.DocumentsToRemove.Select(invalidDocument => 
                             Documents.Find(x => x.DocumentId == invalidDocument.DocumentId && x.VersionId == invalidDocument.VersionId)))
                {
                    Documents.Remove(item);
                }
                
                //now evaluate all incoming documents against the existing tracker record that are not already identified for removal and make sure
                //that anything new is added to the tracker
                foreach (var cmsDocument in from cmsDocument in 
                             arg.IncomingDocuments where !evaluationResults.DocumentsToRemove
                             .Exists(x => x.DocumentId == cmsDocument.DocumentId && x.VersionId == cmsDocument.VersionId) 
                         let item = Documents.Find(x => x.DocumentId == cmsDocument.DocumentId) where item == null select cmsDocument)
                {
                    Documents.Add(new TrackerDocument(cmsDocument.DocumentId, cmsDocument.VersionId, cmsDocument.OriginalFileName));
                }
            }

            return Task.CompletedTask;
        }

        public Task ProcessEvaluatedDocuments()
        {
            Log(LogType.ProcessedEvaluatedDocuments);

            return Task.CompletedTask;
        }
        
        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            document!.PdfBlobName = arg.BlobName;
            document.Status = DocumentStatus.PdfUploadedToBlob;

            Log(LogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }
        
        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            document!.PdfBlobName = arg.BlobName;
            document.Status = DocumentStatus.DocumentAlreadyProcessed;

            Log(LogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentNotFoundInDDEI(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.NotFoundInDDEI;

            Log(LogType.DocumentNotFoundInDDEI, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnableToConvertToPdf;

            Log(LogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnexpectedFailure;

            Log(LogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterNoDocumentsFoundInDDEI()
        {
            Status = TrackerStatus.NoDocumentsFoundInDDEI;
            Log(LogType.NoDocumentsFoundInDDEI);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterIndexed(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.Indexed;

            Log(LogType.Indexed, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.OcrAndIndexFailure;

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

        public Task<List<TrackerDocument>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task<bool> AllDocumentsFailed()
        {
            return Task.FromResult(
                Documents.All(d => d.Status is DocumentStatus.NotFoundInDDEI 
                    or DocumentStatus.UnableToConvertToPdf or DocumentStatus.UnexpectedFailure));
        }

        public Task<bool> IsAlreadyProcessed()
        {
            return Task.FromResult(Status is TrackerStatus.Completed or TrackerStatus.NoDocumentsFoundInDDEI);
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

        private void Log(LogType status, string documentId = null)
        {
            Logs.Add(new Log
            {
                LogType = status.ToString(),
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                DocumentId = documentId
            });
        }

        [FunctionName("Tracker")]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<Tracker>();
        }

        [FunctionName("TrackerStatus")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseUrn}/{caseId}/tracker")] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            const string loggingName = $"TrackerStatus - {nameof(HttpStart)}";
            const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";
            
            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                return new BadRequestObjectResult(correlationErrorMessage);
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId))
                if (currentCorrelationId == Guid.Empty)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

            log.LogMethodEntry(currentCorrelationId, loggingName, caseId);

            var entityId = new EntityId(nameof(Tracker), caseId);
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            log.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            return new OkObjectResult(stateResponse.EntityState);
        }
    }
}