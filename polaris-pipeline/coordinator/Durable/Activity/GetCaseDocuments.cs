using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Extensions;
using Common.Services.DocumentToggle;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Services;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity
{
    public class GetCaseDocuments
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiClientFactory _ddeiClientFactory;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IDocumentToggleService _documentToggleService;
        private readonly IStateStorageService _stateStorageService;
        private readonly ILogger<GetCaseDocuments> _log;
        private readonly IConfiguration _configuration;

        public GetCaseDocuments(
                 IDdeiClient ddeiClient,
                 IDdeiClientFactory ddeiClientFactory,
                 IDdeiArgFactory ddeiArgFactory,
                 IDocumentToggleService documentToggleService,
                 IStateStorageService stateStorageService,
                 ILogger<GetCaseDocuments> logger,
                 IConfiguration configuration)
        {
            _ddeiClient = ddeiClient.ExceptionIfNull();
            _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
            _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
            _documentToggleService = documentToggleService.ExceptionIfNull();
            _stateStorageService = stateStorageService.ExceptionIfNull();
            _log = logger.ExceptionIfNull();
            _configuration = configuration.ExceptionIfNull();
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

            var mdsClient = _ddeiClientFactory.Create(payload.CmsAuthValues, DdeiClients.Mds);

            var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(
                payload.CmsAuthValues,
                payload.CorrelationId,
                payload.Urn,
                payload.CaseId);

            var getDocumentsTask = _ddeiClient.ListDocumentsAsync(arg);
            var getPcdRequestsTask = mdsClient.GetPcdRequestsAsync(arg);
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

            var documents = new GetCaseDocumentsResponse(cmsDocuments, pcdRequests, defendantsAndCharges);
            await _stateStorageService.UpdateCaseDocumentsAsync(payload.CaseId, documents);

            return documents;
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