using System;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Logging;
using Common.ValueObjects;
using coordinator.Durable.Activity;
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
        public async Task<RefreshDocumentResult> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var caseEntity = await CreateOrGetCaseDurableEntity(context, payload.CmsCaseId, false, payload.CorrelationId, log);

            // 1. Get Pdf
            PdfConversionStatus pdfConversionResult = PdfConversionStatus.DocumentConverted;
            try
            {
                pdfConversionResult = await context.CallActivityAsync<PdfConversionStatus>(nameof(GeneratePdf), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestratorNext), $"Error calling {nameof(RefreshDocumentOrchestratorNext)}: {exception.Message}", exception);
                throw;
            }

            caseEntity.SetDocumentFlags((
                payload.PolarisDocumentId.ToString(),
                payload.CmsDocumentTracker.IsOcrProcessed,
                payload.CmsDocumentTracker.IsDispatched
            ));

            // todo: this is temporary code until the coordinator refactor exercise is done.
            if (pdfConversionResult != PdfConversionStatus.DocumentConverted)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                caseEntity.SetDocumentConversionStatus((payload.PolarisDocumentId.ToString(), pdfConversionResult));
                return new RefreshDocumentResult();
            }

            if (payload.DocumentDeltaType == DocumentDeltaType.RequiresPdfRefresh)
            {
                // return and DO NOT set to PdfUploadedToBlob.  If we are refreshing the PDF it is because thr OCR flag has changed.
                //  The document will already either be at PdfUploadedToBlob or Indexed status.  If it is at Indexed status then we do not want to set the
                //  the flag back to PdfUploadedToBlob as Indexed is still correct.  As per comment above, all of this is to be rebuilt in pipeline refresh.
                return new RefreshDocumentResult();
            }

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.PdfUploadedToBlob, payload.BlobName));

            // 2. 
            ExtractTextResult extractTextResult = null;
            try
            {
                extractTextResult = await context.CallActivityAsync<ExtractTextResult>(nameof(ExtractText), payload);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.OcrAndIndexFailure, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestratorNext), $"Error when running {nameof(RefreshDocumentOrchestratorNext)} orchestration: {exception.Message}", exception);
                throw;
            }

            caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.Indexed, payload.BlobName));

            return new RefreshDocumentResult
            {
                OcrLineCount = extractTextResult.LineCount
            };
        }
    }
}
