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
    public class CreateGeneratePdfHttpRequest
    {
        private readonly IGeneratePdfHttpRequestFactory _generatePdfHttpRequestFactory;
        private readonly ILogger<CreateGeneratePdfHttpRequest> _log;

        public CreateGeneratePdfHttpRequest(IGeneratePdfHttpRequestFactory generatePdfHttpRequestFactory, ILogger<CreateGeneratePdfHttpRequest> logger)
        {
            _generatePdfHttpRequestFactory = generatePdfHttpRequestFactory;
            _log = logger;
        }

        [FunctionName("CreateGeneratePdfHttpRequest")]
        public DurableHttpRequest Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(CreateGeneratePdfHttpRequest)} - {nameof(Run)}";
            var payload = context.GetInput<GeneratePdfHttpRequestActivityPayload>();

            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CmsCaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CmsCaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException("DocumentId is empty");
            if (string.IsNullOrWhiteSpace(payload.FileName))
                throw new ArgumentException("The supplied filename is empty");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");

            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());

            var result = _generatePdfHttpRequestFactory.Create(payload.CmsCaseUrn, payload.CmsCaseId, payload.DocumentCategory, payload.DocumentId, payload.FileName, payload.VersionId, payload.CmsAuthValues, payload.CorrelationId);

            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            return result;
        }
    }
}
