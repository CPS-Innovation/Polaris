using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using Common.Logging;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestrator
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<CaseDocumentOrchestrator> _log;
        
        public CaseDocumentOrchestrator(IJsonConvertWrapper jsonConvertWrapper, ILogger<CaseDocumentOrchestrator> log)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _log = log;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            const string loggingName = $"{nameof(CaseDocumentOrchestrator)} - {nameof(Run)}";
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Get the pipeline tracker for DocumentId: '{payload.DocumentId}'");
            var tracker = GetTracker(context, payload.CaseId, payload.CorrelationId, log);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the PDF Generator for DocumentId: '{payload.DocumentId}', FileName: '{payload.FileName}'");
            var pdfGeneratorResponse = await CallPdfGeneratorAsync(context, payload, tracker, log);

            if (!pdfGeneratorResponse.AlreadyProcessed)
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the Text Extractor for DocumentId: '{payload.DocumentId}', FileName: '{payload.FileName}'");
                await CallTextExtractorAsync(context, payload, pdfGeneratorResponse.BlobName, tracker, log);
            }

            log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }
        
        private async Task<GeneratePdfResponse> CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            GeneratePdfResponse response = null;
            
            try
            {
                log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorAsync), payload.ToJson());
                
                response = await CallPdfGeneratorHttpAsync(context, payload, tracker, log);
                
                if (response.AlreadyProcessed)
                {
                    await tracker.RegisterBlobAlreadyProcessed(new RegisterPdfBlobNameArg(payload.DocumentId, payload.VersionId, response.BlobName));
                }

                else
                {
                    await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg(payload.DocumentId, payload.VersionId, response.BlobName));
                }
                
                return response;
            }
            catch (Exception exception)
            {
                await tracker.RegisterUnexpectedPdfDocumentFailure(payload.DocumentId);

                log.LogMethodError(payload.CorrelationId, nameof(CaseDocumentOrchestrator),
                    $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration: {exception.Message}",
                    exception);

                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallPdfGeneratorAsync), response.ToJson());
            }
        }

        private async Task<GeneratePdfResponse> CallPdfGeneratorHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), payload.ToJson());
            
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateGeneratePdfHttpRequest),
                new GeneratePdfHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, payload.DocumentCategory, payload.DocumentId, payload.FileName, payload.VersionId, payload.UpstreamToken, payload.CorrelationId));
            var response = await context.CallHttpAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return _jsonConvertWrapper.DeserializeObject<GeneratePdfResponse>(response.Content);
                case HttpStatusCode.NotFound:
                    await tracker.RegisterDocumentNotFoundInDDEI(payload.DocumentId);
                    break;
                case HttpStatusCode.NotImplemented:
                    await tracker.RegisterUnableToConvertDocumentToPdf(payload.DocumentId);
                    break;
            }
            
            throw new HttpRequestException($"Failed to generate pdf for document id '{payload.DocumentId}'. Status code: {response.StatusCode}.");
        }
        
        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ITracker tracker, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorAsync), payload.ToJson());

            try
            {
                await CallTextExtractorHttpAsync(context, payload, blobName, log);
                await tracker.RegisterIndexed(payload.DocumentId);
            }
            catch (Exception exception)
            {
                await tracker.RegisterOcrAndIndexFailure(payload.DocumentId);

                log.LogMethodError(payload.CorrelationId, nameof(CallTextExtractorAsync), $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration: {exception.Message}", exception);
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
            
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateTextExtractorHttpRequest),
                new TextExtractorHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, payload.DocumentId, payload.VersionId, blobName, payload.CorrelationId));
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                request.Headers.TryGetValue(HttpHeaderKeys.Authorization, out var tokenUsed);
                throw new HttpRequestException($"Failed to ocr/index document with id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}]. CorrelationId: {payload.CorrelationId}");
            }
            
            log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorHttpAsync), string.Empty);
        }
        
        private ITracker GetTracker(IDurableOrchestrationContext context, long caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(GetTracker), $"CaseId: {caseId.ToString()}");
            
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            
            log.LogMethodExit(correlationId, nameof(GetTracker), string.Empty);
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
