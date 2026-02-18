using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Extensions;
using Common.LayerResponse;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public class DdeiCaseOrchestrationService : IDdeiCaseOrchestrationService
{
    private readonly IMdsClient _mdsClient;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly ICaseDetailsMapper _caseDetailsMapper;

    public DdeiCaseOrchestrationService(
            IMdsClient mdsClient,
            IMdsArgFactory mdsArgFactory,
            ICaseDetailsMapper caseDetailsMapper
        )
    {
        _mdsClient = mdsClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _caseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
    }

    public async Task<CaseDto> GetCase(MdsCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseDetails(arg);
        return _caseDetailsMapper.MapCaseDetails(@case);
    }

    public async Task<IEnumerable<CaseDto>> GetCases(MdsUrnArgDto arg)
    {
        var caseIdentifiers = await _mdsClient.ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseDetails(_mdsArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
    }

    private async Task<ILayerResponse<CaseDetailsDto>> GetCaseDetails(MdsCaseIdentifiersArgDto arg)
    {
        var response = new LayerResponse<CaseDetailsDto>();

        var mdsCaseIdOnlyArgDto = new MdsCaseIdOnlyArgDto { CaseId = arg.CaseId, CmsAuthValues = arg.CmsAuthValues, CorrelationId = arg.CorrelationId, Urn = arg.Urn };
        var caseSummaryResponse =  await _mdsClient.GetCaseSummaryAsync(mdsCaseIdOnlyArgDto);
        if (caseSummaryResponse.HasError) return response.AddErrors(caseSummaryResponse);
        
        var defendantsAndChargesResponse = await _mdsClient.GetDefendantAndChargesAsync(arg);
        if (defendantsAndChargesResponse.HasError) return response.AddErrors(defendantsAndChargesResponse);

        var witnessesResponse = await _mdsClient.GetWitnessesAsync(arg);
        if (witnessesResponse.HasError) return response.AddErrors(witnessesResponse);

        var pcdRequestResponse = await _mdsClient.GetPcdRequestsAsync(arg);
        if (pcdRequestResponse.HasError) return response.AddErrors(pcdRequestResponse);

        response.Content = new CaseDetailsDto
        {
            Summary = caseSummaryResponse.Content,
            DefendantsAndCharges = defendantsAndChargesResponse.Content.DefendantsAndCharges,
            Witnesses = MapWitnesses(witnessesResponse.Content),
            PreChargeDecisionRequests = pcdRequestResponse.Content,
        };

        return response;
    }

    private IEnumerable<WitnessDto> MapWitnesses(IEnumerable<BaseCaseWitnessResponse> witnesses) =>
        _caseDetailsMapper.MapWitnesses(witnesses);


}