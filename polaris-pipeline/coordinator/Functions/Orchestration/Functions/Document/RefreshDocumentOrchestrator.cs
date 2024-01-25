using System;
using System.Threading.Tasks;
using Common.Dto.Tracker;
using Common.Logging;
using Common.ValueObjects;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Document;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Document
{
    public class RefreshDocumentOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestrator> _log;

        const string loggingName = $"{nameof(RefreshDocumentOrchestrator)} - {nameof(Run)}";

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
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);

            var caseEntity = await CreateOrGetCaseDurableEntity(context, payload.CmsCaseId, false, payload.CorrelationId, log);

            await CallPdfGeneratorAsync(context, payload, caseEntity, log);

            if (payload.CmsDocumentTracker != null)
            {
                caseEntity.SetDocumentFlags((
                    payload.PolarisDocumentId.ToString(),
                    payload.CmsDocumentTracker.IsOcrProcessed,
                    payload.CmsDocumentTracker.IsDispatched
                ));
            }

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.PdfUploadedToBlob, payload.BlobName));

            await CallTextExtractorAsync(context, payload, payload.BlobName, caseEntity, log);

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.Indexed, payload.BlobName));
        }

        private async Task CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ICaseDurableEntity caseEntity, ILogger log)
        {
            try
            {
                await context.CallActivityAsync(nameof(GeneratePdf), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(RefreshDocumentOrchestrator)}: {exception.Message}", exception);
                throw;
            }
        }

        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ICaseDurableEntity caseEntity, ILogger log)
        {
            try
            {
                await context.CallActivityAsync(nameof(ExtractText), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.OcrAndIndexFailure, null));
                log.LogMethodError(payload.CorrelationId, nameof(CallTextExtractorAsync), $"Error when running {nameof(RefreshDocumentOrchestrator)} orchestration: {exception.Message}", exception);
                throw;
            }
        }
    }
}
