using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using Common.Services.DocumentExtractionService.Contracts;
using coordinator.Domain;
using coordinator.Domain.Tracker;
using coordinator.Mappers;
using coordinator.Services.DocumentToggle;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDocuments
    {
        private readonly IDdeiDocumentExtractionService _documentExtractionService;
        private readonly IDocumentToggleService _documentToggleService;
        private readonly ITransitionDocumentMapper _transitionDocumentMapper;
        private readonly ILogger<GetCaseDocuments> _log;

        const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";

        public GetCaseDocuments(
                 IDdeiDocumentExtractionService documentExtractionService,
                 IDocumentToggleService documentToggleService,
                 ITransitionDocumentMapper transitionDocumentMapper,
                 ILogger<GetCaseDocuments> logger)
        {
            _documentExtractionService = documentExtractionService;
            _documentToggleService = documentToggleService;
            _transitionDocumentMapper = transitionDocumentMapper;
            _log = logger;
        }

        [FunctionName(nameof(GetCaseDocuments))]
        public async Task<TransitionDocument[]> Run([ActivityTrigger] IDurableActivityContext context)
        {
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
            var caseDocuments = await _documentExtractionService.ListDocumentsAsync(
              payload.CmsCaseUrn,
              payload.CmsCaseId.ToString(),
              payload.CmsAuthValues,
              payload.CorrelationId);

            _log.LogMethodExit(payload.CorrelationId, loggingName, caseDocuments.ToJson());
            return caseDocuments
                    .Select(MapToTransitionDocument)
                    .ToArray();
        }

        private TransitionDocument MapToTransitionDocument(CmsCaseDocument document)
        {
            var transitionDocument = _transitionDocumentMapper.MapToTransitionDocument(document);

            transitionDocument.PresentationFlags = _documentToggleService
              .GetDocumentPresentationFlags(transitionDocument);

            return transitionDocument;
        }
    }
}
