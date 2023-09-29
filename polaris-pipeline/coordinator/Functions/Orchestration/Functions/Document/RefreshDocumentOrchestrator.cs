using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Dto.Tracker;
using Common.Logging;
using Common.ValueObjects;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Document;
using coordinator.Functions.DurableEntity.Entity.Contract;
using coordinator.Health;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

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

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Get trackers for CaseId: '{payload.CmsCaseId}'");
            var (caseEntity, caseRefreshLogsEntity) = await CreateOrGetCaseDurableEntities(context, payload.CmsCaseId, false, payload.CorrelationId, log);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the PDF Generator for PolarisDocumentId: '{payload.PolarisDocumentId}'");
            await CallPdfGeneratorAsync(context, payload, caseEntity, caseRefreshLogsEntity, log);

            if (payload.CmsDocumentTracker != null)
                caseEntity.SetOcrProcessed((payload.PolarisDocumentId.ToString(), payload.CmsDocumentTracker.IsOcrProcessed));

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.PdfUploadedToBlob, payload.BlobName));
            caseRefreshLogsEntity.LogDocument((context.CurrentUtcDateTime, DocumentLogType.PdfGenerated, payload.PolarisDocumentId.ToString()));

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the Text Extractor for PolarisDocumentId: '{payload.PolarisDocumentId}', BlobName: '{payload.BlobName}'");
            await CallTextExtractorAsync(context, payload, payload.BlobName, caseEntity, caseRefreshLogsEntity, log);

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.Indexed, payload.BlobName));
            caseRefreshLogsEntity.LogDocument((context.CurrentUtcDateTime, DocumentLogType.Indexed, payload.PolarisDocumentId.ToString()));

            log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }

        private async Task CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ICaseDurableEntity caseEntity, ICaseRefreshLogsDurableEntity caseRefreshLogsEntity, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorAsync), payload.ToJson());
            try
            {
                await context.CallActivityAsync(nameof(GeneratePdf), payload);
            }
            catch (Exception exception)
            {
                GeneratePdfHealthCheck.LastException = exception.Message;
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(RefreshDocumentOrchestrator)}: {exception.Message}", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallPdfGeneratorAsync), null);
            }
        }

        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ICaseDurableEntity caseEntity, ICaseRefreshLogsDurableEntity caseRefreshLogsEntity, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorAsync), payload.ToJson());

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
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorAsync), null);
            }
        }
    }
}
