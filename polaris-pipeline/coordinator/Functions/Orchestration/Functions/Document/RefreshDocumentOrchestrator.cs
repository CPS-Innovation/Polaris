using System;
using System.Threading.Tasks;
using Common.Dto.Tracker;
using Common.Logging;
using Common.ValueObjects;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Document;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Document
{
    public class RefreshDocumentOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestrator> _log;

        public static string GetKey(long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"[{caseId}]-{polarisDocumentId}";
        }

        public RefreshDocumentOrchestrator(ILogger<RefreshDocumentOrchestrator> log)
        {
            _log = log;
        }

        [FunctionName(nameof(RefreshDocumentOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));
            }

            var log = context.CreateReplaySafeLogger(_log);

            var caseEntity = await CreateOrGetCaseDurableEntity(context, payload.CmsCaseId, false);

            try
            {
                await context.CallActivityAsync(nameof(GeneratePdf), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(GeneratePdf)}: {exception.Message}", exception);
                throw;
            }

            if (payload.CmsDocumentTracker != null)
            {
                caseEntity.SetDocumentFlags((
                    payload.PolarisDocumentId.ToString(),
                    payload.CmsDocumentTracker.IsOcrProcessed,
                    payload.CmsDocumentTracker.IsDispatched
                ));
            }

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.PdfUploadedToBlob, payload.BlobName));

            try
            {
                await context.CallActivityAsync(nameof(ExtractText), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.OcrAndIndexFailure, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error when running {nameof(ExtractText)} orchestration: {exception.Message}", exception);
                throw;
            }

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.Indexed, payload.BlobName));
        }
    }
}
