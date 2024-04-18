using System;
using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Logging;
using Common.ValueObjects;

using coordinator.Durable.Activity;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Durable.Orchestration
{
    public class RefreshDocumentOrchestrator : BaseOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestrator> _log;
        const string loggingName = $"{nameof(RefreshDocumentOrchestrator)} - {nameof(Run)}";

        public static string GetKey(long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"[{caseId}]-{polarisDocumentId}";
        }

        public RefreshDocumentOrchestrator(ILogger<RefreshDocumentOrchestrator> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [FunctionName(nameof(RefreshDocumentOrchestrator))]
        public async Task<RefreshDocumentResult> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);

            var caseEntity = await CreateOrGetCaseDurableEntity(context, payload.CmsCaseId, false, payload.CorrelationId, log);

            var isPdfConverted = await CallPdfGeneratorAsync(context, payload, caseEntity, log);

            if (payload.CmsDocumentTracker != null)
            {
                caseEntity.SetDocumentFlags((
                    payload.PolarisDocumentId.ToString(),
                    payload.CmsDocumentTracker.IsOcrProcessed,
                    payload.CmsDocumentTracker.IsDispatched
                ));
            }

            // todo: this is temporary code until the coordinator refactor exercise is done.
            if (!isPdfConverted)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                return new RefreshDocumentResult();
            }

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.PdfUploadedToBlob, payload.BlobName));

            if (payload.DocumentDeltaType != DocumentDeltaType.RequiresIndexing)
            {
                return new RefreshDocumentResult();
            }

            var result = await CallTextExtractorAsync(context, payload, caseEntity, log);

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.Indexed, payload.BlobName));

            return new RefreshDocumentResult
            {
                OcrLineCount = result.LineCount
            };
        }

        private async Task<bool> CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ICaseDurableEntity caseEntity, ILogger log)
        {
            try
            {
                return await context.CallActivityAsync<bool>(nameof(GeneratePdf), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(RefreshDocumentOrchestrator)}: {exception.Message}", exception);
                throw;
            }
        }

        private async Task<ExtractTextResult> CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ICaseDurableEntity caseEntity, ILogger log)
        {
            try
            {
                return await context.CallActivityAsync<ExtractTextResult>(nameof(ExtractText), payload);
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
