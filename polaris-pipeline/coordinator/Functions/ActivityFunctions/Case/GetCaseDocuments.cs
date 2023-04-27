using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Logging;
using Common.Services.DocumentToggle;
using coordinator.Domain;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions.Case
{
    public class GetCaseDocuments
    {
        private readonly IDdeiClient _ddeiService;
        private readonly IDocumentToggleService _documentToggleService;
        private readonly ILogger<GetCaseDocuments> _log;

        const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";

        public GetCaseDocuments(
                 IDdeiClient ddeiService,
                 IDocumentToggleService documentToggleService,
                 ILogger<GetCaseDocuments> logger)
        {
            _ddeiService = ddeiService;
            _documentToggleService = documentToggleService;
            _log = logger;
        }

        [FunctionName(nameof(GetCaseDocuments))]
        public async Task<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests)> Run([ActivityTrigger] IDurableActivityContext context)
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
            var caseDocuments = await _ddeiService.ListDocumentsAsync(
              payload.CmsCaseUrn,
              payload.CmsCaseId.ToString(),
              payload.CmsAuthValues,
              payload.CorrelationId);

            _log.LogMethodExit(payload.CorrelationId, loggingName, caseDocuments.ToJson());
            return (caseDocuments
                    .Select(MapPresentationFlags)
                    .ToArray(), new PcdRequestDto[0]);
        }

        private DocumentDto MapPresentationFlags(DocumentDto document)
        {
            document.PresentationFlags = _documentToggleService.GetDocumentPresentationFlags(document);

            return document;
        }

        private PcdRequestDto MapPresentationFlags(PcdRequestDto pcdRequest)
        {
            pcdRequest.PresentationFlags = _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest);

            return pcdRequest;
        }
        private DefendantAndChargesDto MapPresentationFlags(DefendantAndChargesDto defendantAndCharges)
        {
            defendantAndCharges.PresentationFlags = _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges);

            return defendantAndCharges;
        }
    }
}
