using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Response;
using Common.Logging;
using Common.ValueObjects;
using coordinator.Durable.Activity;
using coordinator.Durable.Activity.ExtractTextNext;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Durable.Orchestration
{
    public class RefreshDocumentOrchestratorNext : BaseOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestratorNext> _log;
        const string loggingName = $"{nameof(RefreshDocumentOrchestratorNext)} - {nameof(Run)}";

        public static string GetKey(long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"[{caseId}]-{polarisDocumentId}";
        }

        public RefreshDocumentOrchestratorNext(ILogger<RefreshDocumentOrchestratorNext> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [FunctionName(nameof(RefreshDocumentOrchestratorNext))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            var log = context.CreateReplaySafeLogger(_log);
            var caseEntity = CreateOrGetCaseDurableEntity(context, payload.CmsCaseId);

            // 1. Get Pdf
            try
            {
                var pdfConversionResult = await context.CallActivityAsync<PdfConversionStatus>(nameof(GeneratePdf), payload);
                if (pdfConversionResult != PdfConversionStatus.DocumentConverted)
                {
                    caseEntity.SetDocumentPdfConversionFailed((payload.PolarisDocumentId.ToString(), pdfConversionResult));
                    return; // why return here ....
                }
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentPdfConversionFailed((payload.PolarisDocumentId.ToString(), PdfConversionStatus.UnexpectedError));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestratorNext), $"Error calling {nameof(RefreshDocumentOrchestratorNext)}: {exception.Message}", exception);
                return;
            }

            // todo: this is temporary code until the coordinator refactor exercise is done.

            if (payload.DocumentDeltaType != DocumentDeltaType.RequiresIndexing)
            {
                // return and DO NOT set to PdfUploadedToBlob.  If we are refreshing the PDF it is because thr OCR flag has changed.
                //  The document will already either be at PdfUploadedToBlob or Indexed status.  If it is at Indexed status then we do not want to set the
                //  the flag back to PdfUploadedToBlob as Indexed is still correct.  As per comment above, all of this is to be rebuilt in pipeline refresh.
                return;
            }

            caseEntity.SetDocumentPdfConversionSucceeded(payload.PolarisDocumentId.ToString());

            try
            {
                var operationId = await context.CallActivityAsync<Guid>(nameof(InitiateOcr), (payload.BlobName, payload.CorrelationId));
                await PollActivityUntilComplete(context, nameof(CompleteOcr), (operationId, payload.OcrBlobName, payload.CorrelationId));

                var storeIndexesResult = await context.CallActivityAsync<StoreCaseIndexesResult>(nameof(StoreIndex), payload);
                await PollActivityUntilComplete(context, nameof(CheckIndexStored), (payload, storeIndexesResult.LineCount));
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentIndexingFailed(payload.PolarisDocumentId.ToString());
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestratorNext), $"Error when running {nameof(RefreshDocumentOrchestratorNext)} orchestration: {exception.Message}", exception);
                return;
            }

            caseEntity.SetDocumentIndexingSucceeded(payload.PolarisDocumentId.ToString());
        }

        private async Task PollActivityUntilComplete(IDurableOrchestrationContext context, string activityName, object activityInput)
        {
            while (true)
            {
                var nextCheck = context.CurrentUtcDateTime.AddMilliseconds(2000); // todo: to constant
                await context.CreateTimer(nextCheck, CancellationToken.None);

                var isCompleted = await context.CallActivityAsync<bool>(activityName, activityInput);
                if (isCompleted)
                {
                    break;
                }
            }
        }
    }
}
