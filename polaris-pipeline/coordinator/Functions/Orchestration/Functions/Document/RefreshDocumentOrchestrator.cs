using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Logging;
using Common.Wrappers.Contracts;
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
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<RefreshDocumentOrchestrator> _log;

        const string loggingName = $"{nameof(RefreshDocumentOrchestrator)} - {nameof(Run)}";

        public RefreshDocumentOrchestrator(IJsonConvertWrapper jsonConvertWrapper, ILogger<RefreshDocumentOrchestrator> log)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
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
            var pdfGeneratorResponse = await CallPdfGeneratorAsync(context, payload, caseEntity, caseRefreshLogsEntity, log);

            if (pdfGeneratorResponse.AlreadyProcessed)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.DocumentAlreadyProcessed, pdfGeneratorResponse.BlobName));
                caseRefreshLogsEntity.LogDocument((context.CurrentUtcDateTime, DocumentLogType.PdfAlreadyGenerated, payload.PolarisDocumentId.ToString()));
            }
            else 
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.PdfUploadedToBlob, pdfGeneratorResponse.BlobName));
                caseRefreshLogsEntity.LogDocument((context.CurrentUtcDateTime, DocumentLogType.PdfGenerated, payload.PolarisDocumentId.ToString()));

                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the Text Extractor for PolarisDocumentId: '{payload.PolarisDocumentId}', BlobName: '{pdfGeneratorResponse.BlobName}'");
                await CallTextExtractorAsync(context, payload, pdfGeneratorResponse.BlobName, caseEntity, caseRefreshLogsEntity, log);

                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.Indexed, pdfGeneratorResponse.BlobName));
                caseRefreshLogsEntity.LogDocument((context.CurrentUtcDateTime, DocumentLogType.Indexed, payload.PolarisDocumentId.ToString()));
            }

            log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }

        private async Task<GeneratePdfResponse> CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ICaseDurableEntity caseEntity, ICaseRefreshLogsDurableEntity caseRefreshLogsEntity, ILogger log)
        {
            GeneratePdfResponse response = null;

            try
            {
                log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorAsync), payload.ToJson());

                response = await context.CallActivityAsync<GeneratePdfResponse>(nameof(GeneratePdf), payload);
                if(response == null)
                    throw new ArgumentNullException(nameof(response));  

                return response;
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.UnableToConvertToPdf, null));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(RefreshDocumentOrchestrator)}: {exception.Message}", exception);

                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallPdfGeneratorAsync), response.ToJson());
            }
        }

        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ICaseDurableEntity caseEntity, ICaseRefreshLogsDurableEntity caseRefreshLogsEntity, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorAsync), payload.ToJson());

            try
            {
                await CallTextExtractorHttpAsync(context, payload, blobName, log);
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentStatus((payload.PolarisDocumentId.ToString(), DocumentStatus.OcrAndIndexFailure, null));

                log.LogMethodError(payload.CorrelationId, nameof(CallTextExtractorAsync), $"Error when running {nameof(RefreshDocumentOrchestrator)} orchestration: {exception.Message}", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorAsync), string.Empty);
            }
        }

        private async Task CallTextExtractorHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorHttpAsync), payload.ToJson());

            var request = await context.CallActivityAsync<DurableHttpRequest>
                (
                    nameof(CreateTextExtractorHttpRequest),
                    new TextExtractorHttpRequestActivityPayload
                    (
                        payload.PolarisDocumentId,
                        payload.CmsCaseUrn,
                        payload.CmsCaseId,
                        payload.CmsDocumentId,
                        payload.CmsVersionId,
                        blobName,
                        payload.CorrelationId
                    )
                );
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Failed to OCR/Index document with id '{payload.CmsDocumentId}'. Status code: {response.StatusCode}. CorrelationId: {payload.CorrelationId}");

            log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorHttpAsync), string.Empty);
        }
    }
}
