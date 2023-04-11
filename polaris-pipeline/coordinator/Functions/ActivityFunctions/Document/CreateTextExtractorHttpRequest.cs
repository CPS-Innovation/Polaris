using System;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions.Document
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

        [FunctionName(nameof(CreateTextExtractorHttpRequest))]
        public DurableHttpRequest Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(CreateTextExtractorHttpRequest)} - {nameof(Run)}";
            var payload = context.GetInput<TextExtractorHttpRequestActivityPayload>();

            if (payload == null)
                throw new ArgumentException($"{nameof(payload)} cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CmsCaseUrn))
                throw new ArgumentException($"{nameof(payload.CmsCaseUrn)} cannot be empty");
            if (payload.CmsCaseId == 0)
                throw new ArgumentException($"{nameof(payload.CmsCaseId)} cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException($"{nameof(payload.DocumentId)} is empty");
            if (string.IsNullOrWhiteSpace(payload.BlobName))
                throw new ArgumentException($"The supplied {nameof(payload.BlobName)} is empty");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException($"{nameof(payload.CorrelationId)} must be valid GUID");

            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            var result = _textExtractorHttpRequestFactory.Create(payload.PolarisDocumentId, payload.CmsCaseId, payload.DocumentId, payload.VersionId, payload.BlobName, payload.CorrelationId);

            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            return result;
        }
    }
}
