using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class CreateDocumentEvaluationHttpRequest
    {
        private readonly IDocumentEvaluationHttpRequestFactory _documentEvaluationHttpRequestFactory;
        private readonly ILogger<CreateDocumentEvaluationHttpRequest> _log;

        public CreateDocumentEvaluationHttpRequest(IDocumentEvaluationHttpRequestFactory documentEvaluationHttpRequestFactory, ILogger<CreateDocumentEvaluationHttpRequest> logger)
        {
            _documentEvaluationHttpRequestFactory = documentEvaluationHttpRequestFactory;
           _log = logger;
        }

        [FunctionName("CreateDocumentEvaluationHttpRequest")]
        public async Task<DurableHttpRequest> Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(CreateDocumentEvaluationHttpRequest)} - {nameof(Run)}";
            var payload = context.GetInput<DocumentEvaluationActivityPayload>();

            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (payload.DocumentsToRemove?.Count == 0)
                throw new ArgumentException("No documents-to-remove nor documents-to-update were supplied to this activity call");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            var result = await _documentEvaluationHttpRequestFactory.Create(payload.CaseUrn, payload.CaseId, payload.DocumentsToRemove, payload.CorrelationId);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            return result;
        }
    }
}
