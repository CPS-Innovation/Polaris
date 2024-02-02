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
            var caseEntity = await GetOrCreateCaseDurableEntity(context, payload.CmsCaseId, false);

            async Task tryCallActivityAsync(string name, DocumentStatus successStatus, DocumentStatus failStatus)
            {
                try
                {
                    await context.CallActivityAsync(name, payload);
                    caseEntity.SetDocumentStatus((payload.PolarisDocumentId, successStatus));
                }
                catch (Exception exception)
                {
                    caseEntity.SetDocumentStatus((payload.PolarisDocumentId, failStatus));
                    log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {name}: {exception.Message}", exception);
                    throw;
                }
            }

            await tryCallActivityAsync(nameof(GeneratePdf), DocumentStatus.PdfUploadedToBlob, DocumentStatus.UnableToConvertToPdf);
            caseEntity.SetDocumentPdfBlobName((payload.PolarisDocumentId, payload.BlobName));

            await tryCallActivityAsync(nameof(ExtractText), DocumentStatus.Indexed, DocumentStatus.OcrAndIndexFailure);
        }
    }
}
