using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Extensions;
using Common.Mappers;
using Common.Services.DocumentToggle;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Services;
using Ddei.Factories;
using Ddei.Mappers;
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
    private readonly IMasterDataServiceClient _masterDataServiceClient;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly ICaseDetailsMapper _caseDetailsMapper;
    private readonly ICmsDocumentDtoMapper _cmsDocumentDtoMapper;
    private readonly IDocumentToggleService _documentToggleService;
    private readonly IStateStorageService _stateStorageService;
    private readonly ILogger<GetCaseDocuments> _log;

    public GetCaseDocuments(
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        IMdsArgFactory mdsArgFactory,
        ICaseDetailsMapper caseDetailsMapper,
        ICmsDocumentDtoMapper cmsDocumentDtoMapper,
        IDocumentToggleService documentToggleService,
        IStateStorageService stateStorageService,
        ILogger<GetCaseDocuments> logger)
    {
        _mdsClient = mdsClient.ExceptionIfNull();
        _masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _caseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
        _cmsDocumentDtoMapper = cmsDocumentDtoMapper.ExceptionIfNull();
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

        var arg = _mdsArgFactory.CreateCaseIdentifiersArg(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId);

        var getDocumentsTask = _masterDataServiceClient.ListDocumentsAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getPcdRequestsTask = _masterDataServiceClient.GetCasePcdRequestsAsync(
            arg,
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getDefendantsAndChargesTask = _masterDataServiceClient.GetCaseDefendantsAsync(
            new ListCaseDefendantsRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var getDocumentsTaskMapped = getDocumentsTask.Result.Select(x => _cmsDocumentDtoMapper.Map(x, null)).ToList();

        var cmsDocuments = getDocumentsTaskMapped
            .Select(MapPresentationFlags)
            .ToArray();

        var pcdRequests = getPcdRequestsTask.Result
            .Select(MapPresentationFlags)
            .ToArray();

        var defendantsAndCharges = getDefendantsAndChargesTask.Result;
        var defendandAndChargesMapped = _caseDetailsMapper.MapDefendantsResponseToDefendantsAndChargesListDto(defendantsAndCharges, arg.CaseId);
        MapPresentationFlags(defendandAndChargesMapped);

        var documents = new GetCaseDocumentsResponse(cmsDocuments, pcdRequests, defendandAndChargesMapped);
        await _stateStorageService.UpdateCaseDocumentsAsync(payload.CaseId, documents);

        return documents;
    }

    private CmsDocumentDto MapPresentationFlags(CmsDocumentDto document)
    {
        document.PresentationFlags = _documentToggleService.GetDocumentPresentationFlags(document);
        return document;
    }

    // need to refactor this if we add PresentationFlags to MDS response and HK PcdRequestDto
    private PcdRequestCoreDto MapPresentationFlags(Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto pcdRequest)
    {
        PcdRequestCoreDto pcdRequestCoreDto = new PcdRequestCoreDto
        {
            Id = pcdRequest.Id,
            DecisionRequiredBy = pcdRequest.DecisionRequiredBy,
            DecisionRequested = pcdRequest.DecisionRequested,
        };
        pcdRequestCoreDto.PresentationFlags = _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest);
        return pcdRequestCoreDto;
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
