using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using Common.Services.DocumentExtractionService.Contracts;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDocuments
    {
        private readonly IDdeiDocumentExtractionService _documentExtractionService;
        private readonly ILogger<GetCaseDocuments> _log;

        public GetCaseDocuments(IDdeiDocumentExtractionService documentExtractionService, ILogger<GetCaseDocuments> logger)
        {
            _documentExtractionService = documentExtractionService;
            _log = logger;
        }

        [FunctionName("GetCaseDocuments")]
        public async Task<CmsCaseDocument[]> Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";
            var payload = context.GetInput<GetCaseDocumentsActivityPayload>();

            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CmsCaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CmsCaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.CmsAuthValues))
                throw new ArgumentException("Cms Auth Token cannot be null");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");

            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            var caseDocuments = await _documentExtractionService.ListDocumentsAsync(payload.CmsCaseUrn, payload.CmsCaseId.ToString(), payload.CmsAuthValues, payload.CorrelationId);

            _log.LogMethodExit(payload.CorrelationId, loggingName, caseDocuments.ToJson());
            return caseDocuments;
        }
    }
}
