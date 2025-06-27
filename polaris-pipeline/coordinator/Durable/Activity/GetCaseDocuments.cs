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
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class GetCaseDocuments
{
    private readonly IMdsClient _mdsClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDocumentToggleService _documentToggleService;
    private readonly IStateStorageService _stateStorageService;
    private readonly ILogger<GetCaseDocuments> _log;

    public GetCaseDocuments(
        IMdsClient mdsClient,
        IDdeiArgFactory ddeiArgFactory,
        IDocumentToggleService documentToggleService,
        IStateStorageService stateStorageService,
        ILogger<GetCaseDocuments> logger)
    {
        _mdsClient = mdsClient.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _documentToggleService = documentToggleService.ExceptionIfNull();
        _stateStorageService = stateStorageService.ExceptionIfNull();
        _log = logger.ExceptionIfNull();
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

        var getDocumentsTask = _mdsClient.ListDocumentsAsync(arg);
        var getPcdRequestsTask = _mdsClient.GetPcdRequestsCoreAsync(arg);
        var getDefendantsAndChargesTask = _mdsClient.GetDefendantAndChargesAsync(arg);

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