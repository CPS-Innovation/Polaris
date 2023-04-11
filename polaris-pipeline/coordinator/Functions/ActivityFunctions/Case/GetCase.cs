// WIP  - TODO for PCD

//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Common.Domain.Extensions;
//using Common.Dto.Document;
//using Common.Logging;
//using Common.Services.DocumentExtractionService.Contracts;
//using coordinator.Domain;
//using coordinator.Domain.Tracker;
//using coordinator.Mappers;
//using coordinator.Services.DocumentToggle;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.DurableTask;
//using Microsoft.Extensions.Logging;

//namespace coordinator.Functions.ActivityFunctions.Case
//{
//    public class GetCase
//    {
//        private readonly IDdeiService _ddeiService;
//        private readonly ILogger<GetCase> _log;

//        const string loggingName = $"{nameof(GetCase)} - {nameof(Run)}";

//        public GetCase(IDdeiService ddeiService, ILogger<GetCase> logger)
//        {
//            _ddeiService = ddeiService;
//            _log = logger;
//        }

//        [FunctionName(nameof(GetCase))]
//        public async Task<TransitionDocumentDto[]> Run([ActivityTrigger] IDurableActivityContext context)
//        {
//            var payload = context.GetInput<GetCaseActivityPayload>();

//            if (payload == null)
//                throw new ArgumentException("Payload cannot be null.");
//            if (string.IsNullOrWhiteSpace(payload.CmsCaseUrn))
//                throw new ArgumentException("CaseUrn cannot be empty");
//            if (payload.CmsCaseId == 0)
//                throw new ArgumentException("CaseId cannot be zero");
//            if (string.IsNullOrWhiteSpace(payload.CmsAuthValues))
//                throw new ArgumentException("Cms Auth Token cannot be null");
//            if (payload.CorrelationId == Guid.Empty)
//                throw new ArgumentException("CorrelationId must be valid GUID");

//            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
//            var case = await _ddeiService.GetCase(
//              payload.CmsCaseUrn,
//              payload.CmsCaseId.ToString(),
//              payload.CmsAuthValues,
//              payload.CorrelationId);

//            _log.LogMethodExit(payload.CorrelationId, loggingName, caseDocuments.ToJson());
//            return caseDocuments
//                    .Select(MapToTransitionDocument)
//                    .ToArray();
//        }

//        private TransitionDocumentDto MapToTransitionDocument(DocumentDto document)
//        {
//            var transitionDocument = _transitionDocumentMapper.MapToTransitionDocument(document);

//            transitionDocument.PresentationFlags = _documentToggleService
//              .GetDocumentPresentationFlags(transitionDocument);

//            return transitionDocument;
//        }
//    }
//}
