using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using coordinator.Services.DocumentToggle;
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
        public async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListDto DefendantAndCharges)> Run([ActivityTrigger] IDurableActivityContext context)
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

            var getPcdRequestsTask = _ddeiClient.GetPcdRequests(arg);
            var getDefendantsAndChargesTask = _ddeiClient.GetDefendantAndCharges(arg);

            await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

            var cmsDocuments = getDocumentsTask.Result
                .Select(doc => MapPresentationFlags(doc))
                .ToArray();


            var pcdRequests = getPcdRequestsTask.Result
                .Select(corePcd => MapPresentationFlags(corePcd))
                .ToArray();

            var defendantsAndChargesResult = getDefendantsAndChargesTask.Result;


            var defendantsAndCharges = new DefendantsAndChargesListDto
            {
                CaseId = payload.CmsCaseId,
                DefendantsAndCharges = defendantsAndChargesResult.OrderBy(dac => dac.ListOrder)
            };

            defendantsAndCharges.PresentationFlags = _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantsAndCharges);

            return (cmsDocuments, pcdRequests, defendantsAndCharges);
        }

        private CmsDocumentDto MapPresentationFlags(CmsDocumentDto document)
        {
            document.PresentationFlags = _documentToggleService.GetDocumentPresentationFlags(document);

            return document;
        }

        private PcdRequestCoreDto MapPresentationFlags(PcdRequestCoreDto pcdRequest)
        {
            pcdRequest.PresentationFlags = _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest);

            return pcdRequest;
        }
    }
}
