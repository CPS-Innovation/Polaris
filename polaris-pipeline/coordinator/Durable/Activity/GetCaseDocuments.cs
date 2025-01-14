using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Services.DocumentToggle;
using Ddei;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using Microsoft.Azure.Functions.Worker;
using coordinator.Domain;

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

        [Function(nameof(GetCaseDocuments))]
        public async Task<GetCaseDocumentsResponse> Run([ActivityTrigger] CasePayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.Urn))
            {
                throw new ArgumentException("CaseUrn cannot be empty");
            }

            if (payload.CaseId == 0)
            {
                throw new ArgumentException("CaseId cannot be zero");
            }

            if (string.IsNullOrWhiteSpace(payload.CmsAuthValues))
            {
                throw new ArgumentException("Cms Auth Token cannot be null");
            }

            if (payload.CorrelationId == Guid.Empty)
            {
                throw new ArgumentException("CorrelationId must be valid GUID");
            }

            var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(
                payload.CmsAuthValues,
                payload.CorrelationId,
                payload.Urn,
                payload.CaseId);

            var getDocumentsTask = _ddeiClient.ListDocumentsAsync(arg);
            var getPcdRequestsTask = _ddeiClient.GetPcdRequestsAsync(arg);
            var getDefendantsAndChargesTask = _ddeiClient.GetDefendantAndChargesAsync(arg);

            await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

            var cmsDocuments = getDocumentsTask.Result
                .Select(MapPresentationFlags)
                .ToArray();


            var pcdRequests = getPcdRequestsTask.Result
                .Select(MapPresentationFlags)
                .ToArray();

            var defendantsAndCharges = getDefendantsAndChargesTask.Result;
            MapPresentationFlags(defendantsAndCharges);

            return new(cmsDocuments, pcdRequests, defendantsAndCharges);
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

        private DefendantsAndChargesListDto MapPresentationFlags(DefendantsAndChargesListDto defendantsAndCharges)
        {
            if (defendantsAndCharges == null)
            {
                return null;
            }

            defendantsAndCharges.PresentationFlags = _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantsAndCharges);
            return defendantsAndCharges;
        }
    }
}