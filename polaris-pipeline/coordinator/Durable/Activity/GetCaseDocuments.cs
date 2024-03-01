using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using coordinator.Services.DocumentToggle;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using coordinator.Durable.Payloads;
using Ddei.Factories;

namespace coordinator.Durable.Activity
{
    public class GetCaseDocuments
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IDocumentToggleService _documentToggleService;
        private readonly ILogger<GetCaseDocuments> _log;
        private readonly IConfiguration _configuration;

        public GetCaseDocuments(
                 IDdeiClient ddeiClient,
                 IDdeiArgFactory ddeiArgFactory,
                 IDocumentToggleService documentToggleService,
                 ILogger<GetCaseDocuments> logger,
                 IConfiguration configuration)
        {
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _documentToggleService = documentToggleService;
            _log = logger;
            _configuration = configuration;
        }

        [FunctionName(nameof(GetCaseDocuments))]
        public async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantAndCharges)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<GetCaseDocumentsActivityPayload>();

            if (string.IsNullOrWhiteSpace(payload.CmsCaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CmsCaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.CmsAuthValues))
                throw new ArgumentException("Cms Auth Token cannot be null");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");

            var getDocumentsTask = _ddeiClient.ListDocumentsAsync(
                payload.CmsCaseUrn,
                payload.CmsCaseId.ToString(),
                payload.CmsAuthValues,
                payload.CorrelationId
            );

            var arg = _ddeiArgFactory.CreateCaseArg(
                payload.CmsAuthValues,
                payload.CorrelationId,
                payload.CmsCaseUrn,
                payload.CmsCaseId);

            var getCaseTask = _ddeiClient.GetCaseAsync(arg);

            await Task.WhenAll(getDocumentsTask, getCaseTask);

            var cmsDocuments = getDocumentsTask.Result
                .Select(doc => MapPresentationFlags(doc))
                .ToArray();

            // todo: rather than a call the get case, we should consider making separate calls to the 
            //  pcd and defendants endpoints. 
            var @case = getCaseTask.Result;

            var pcdRequests = @case.PreChargeDecisionRequests
                       .Select(MapPresentationFlags)
                       .ToArray();

            var defendantsAndCharges = new DefendantsAndChargesListDto
            {
                CaseId = @case.Id,
                DefendantsAndCharges = @case.DefendantsAndCharges.OrderBy(dac => dac.ListOrder)
            };

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
