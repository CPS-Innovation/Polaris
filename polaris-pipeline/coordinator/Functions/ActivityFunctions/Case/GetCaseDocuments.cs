using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Services.DocumentToggle;
using coordinator.Domain;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions.Case
{
    public class GetCaseDocuments
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDocumentToggleService _documentToggleService;
        private readonly ILogger<GetCaseDocuments> _log;
        private readonly IConfiguration _configuration;

        const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";

        public GetCaseDocuments(
                 IDdeiClient ddeiClient,
                 IDocumentToggleService documentToggleService,
                 ILogger<GetCaseDocuments> logger,
                 IConfiguration configuration)
        {
            _ddeiClient = ddeiClient;
            _documentToggleService = documentToggleService;
            _log = logger;
            _configuration = configuration;
        }

        [FunctionName(nameof(GetCaseDocuments))]
        public async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantAndCharges)> Run([ActivityTrigger] IDurableActivityContext context)
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

            var documents = await _ddeiClient.ListDocumentsAsync(payload.CmsCaseUrn, payload.CmsCaseId.ToString(), payload.CmsAuthValues, payload.CorrelationId);

            var cmsDocuments = documents
                .Select(doc => MapPresentationFlags(doc))
                .ToArray();

            var caseArgDto = new DdeiCmsCaseArgDto
            {
                Urn = payload.CmsCaseUrn,
                CaseId = payload.CmsCaseId,
                CmsAuthValues = payload.CmsAuthValues,
                CorrelationId = payload.CorrelationId
            };
            var @case = await _ddeiClient.GetCase(caseArgDto);

            var pcdRequests = @case.PreChargeDecisionRequests
                       .Select(MapPresentationFlags)
                       .ToArray();

            var defendantsAndCharges = new DefendantsAndChargesListDto { CaseId = @case.Id, DefendantsAndCharges = @case.DefendantsAndCharges.OrderBy(dac => dac.ListOrder) };
            defendantsAndCharges.PresentationFlags = _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantsAndCharges);

            return (cmsDocuments, pcdRequests, defendantsAndCharges);
        }

        private CmsDocumentDto MapPresentationFlags(CmsDocumentDto document)
        {
            document.PresentationFlags = _documentToggleService.GetDocumentPresentationFlags(document);
            return document;
        }

        private PcdRequestDto MapPresentationFlags(PcdRequestDto pcdRequest)
        {
            pcdRequest.PresentationFlags = _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest);
            return pcdRequest;
        }
    }
}
