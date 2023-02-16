using System;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class CreateTextExtractorHttpRequest
    {
        private readonly ITextExtractorHttpRequestFactory _textExtractorHttpRequestFactory;
        private readonly ILogger<CreateTextExtractorHttpRequest> _log;

        public CreateTextExtractorHttpRequest(ITextExtractorHttpRequestFactory textExtractorHttpRequestFactory, ILogger<CreateTextExtractorHttpRequest> logger)
        {
           _textExtractorHttpRequestFactory = textExtractorHttpRequestFactory;
           _log = logger;
        }

        [FunctionName("CreateTextExtractorHttpRequest")]
        public DurableHttpRequest Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(CreateTextExtractorHttpRequest)} - {nameof(Run)}";
            var payload = context.GetInput<TextExtractorHttpRequestActivityPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException("DocumentId is empty");
            if (string.IsNullOrWhiteSpace(payload.BlobName))
                throw new ArgumentException("The supplied blob name is empty");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            var result = _textExtractorHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.VersionId, payload.BlobName, payload.CorrelationId);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            return result;
        }
    }
}
