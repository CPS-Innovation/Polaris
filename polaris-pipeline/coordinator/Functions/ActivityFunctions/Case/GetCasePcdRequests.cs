using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Logging;
using Common.Services.DocumentToggle;
using coordinator.Domain;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions.Case
{
    public class GetCasePcdRequests
    {
        private readonly IDdeiClient _ddeiService;
        private readonly IDocumentToggleService _documentToggleService;
        private readonly ILogger<GetCasePcdRequests> _log;

        const string loggingName = $"{nameof(GetCasePcdRequests)} - {nameof(Run)}";

        public GetCasePcdRequests(
                 IDdeiClient ddeiService,
                 IDocumentToggleService documentToggleService,
                 ILogger<GetCasePcdRequests> logger)
        {
            _ddeiService = ddeiService;
            _documentToggleService = documentToggleService;
            _log = logger;
        }

        [FunctionName(nameof(GetCasePcdRequests))]
        public async Task<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantAndChargesDto[] DefendantsAndCharges)> Run([ActivityTrigger] IDurableActivityContext context)
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
            DocumentDto[] documents = await _ddeiService.ListDocumentsAsync(payload.CmsCaseUrn, payload.CmsCaseId.ToString(), payload.CmsAuthValues, payload.CorrelationId);

            var cmsDocuments =
                documents
                    .Select(doc => MapPresentationFlags(doc))
                    .ToArray();

            var caseArgDto = new DdeiCmsCaseArgDto
            {
                Urn = payload.CmsCaseUrn,
                CaseId = payload.CmsCaseId,
                CmsAuthValues = payload.CmsAuthValues,
                CorrelationId = payload.CorrelationId
            };
            var @case = await _ddeiService.GetCase(caseArgDto);

            var pcdRequests
                = @case.PreChargeDecisionRequests
                    .Select(MapPresentationFlags)
                    .ToArray();
            var defendantsAndCharges 
                = @case.DefendantsAndCharges
                    .Select(MapPresentationFlags)
                    .ToArray();

            return (documents, pcdRequests, defendantsAndCharges);
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
