using System;
using System.Threading.Tasks;
using Common.Dto.Tracker;
using Common.Logging;
using Common.ValueObjects;
using coordinator.Durable.Payloads;
using coordinator.Durable.Entity;
using coordinator.Durable.ActivityFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Durable.Orchestration
{
    public class RefreshDocumentOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestrator> _log;

        public static string GetKey(long caseId, PolarisDocumentId polarisDocumentId) => $"[{caseId}]-{polarisDocumentId}";

        public RefreshDocumentOrchestrator(ILogger<RefreshDocumentOrchestrator> log)
        {
            _log = log;
        }

        [FunctionName(nameof(RefreshDocumentOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>()
                ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var caseEntityProxy = CaseDurableEntity.GetEntityProxyInterface(context, payload.CmsCaseId);

            async Task TryCallActivityAsync(string name, DocumentStatus successStatus, DocumentStatus failStatus)
            {
                try
                {
                    await context.CallActivityAsync(name, payload);
                    caseEntityProxy.SetDocumentStatus((payload.PolarisDocumentId, successStatus));
                }
                catch (Exception exception)
                {
                    caseEntityProxy.SetDocumentStatus((payload.PolarisDocumentId, failStatus));
                    log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {name}: {exception.Message}", exception);
                    throw;
                }
            }

            await TryCallActivityAsync(
                name: nameof(GeneratePdf),
                successStatus: DocumentStatus.PdfUploadedToBlob,
                failStatus: DocumentStatus.UnableToConvertToPdf
            );

            caseEntityProxy.SetDocumentPdfBlobName((payload.PolarisDocumentId, payload.BlobName));

            await TryCallActivityAsync(
                name: nameof(ExtractText),
                successStatus: DocumentStatus.Indexed,
                failStatus: DocumentStatus.OcrAndIndexFailure
            );
        }
    }
}
