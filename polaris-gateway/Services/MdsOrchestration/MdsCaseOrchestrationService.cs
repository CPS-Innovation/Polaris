using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using Newtonsoft.Json;
using PolarisGateway.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Services.MdsOrchestration;

public class MdsCaseOrchestrationService(IMdsClient mdsClient,
                            IMasterDataServiceClient masterDataServiceClient,
                            IMdsArgFactory mdsArgFactory,
                            ICaseDetailsMapper caseDetailsMapper) : IMdsCaseOrchestrationService
{
    private readonly IMdsClient _mdsClient = mdsClient;
    private readonly IMasterDataServiceClient _masterDataServiceClient = masterDataServiceClient;
    private readonly IMdsArgFactory _mdsArgFactory = mdsArgFactory;
    private readonly ICaseDetailsMapper _caseDetailsMapper = caseDetailsMapper;

    public async Task<CaseDto> GetCase(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var @case = await GetCaseDetails(arg, cancellationToken);
        return _caseDetailsMapper.MapCaseDetails(@case);
    }

    public async Task<IEnumerable<CaseDto>> GetCases(MdsUrnArgDto arg, CancellationToken cancellationToken = default)
    {
        var caseIdentifiers = await _mdsClient.ListCaseIdsAsync(arg, cancellationToken);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseDetails(_mdsArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id), cancellationToken));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
    }

    private async Task<CaseDetailsDto> GetCaseDetails(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var caseSummaryRequest = new GetCaseSummaryRequest(arg.CaseId, arg.CorrelationId);

        var cmsAuthValues = new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId);

        var getCaseSummaryTask = _masterDataServiceClient.GetCaseSummaryAsync(caseSummaryRequest, cmsAuthValues, cancellationToken);
        var witnessesTask = _mdsClient.GetWitnessesAsync(arg, cancellationToken);
        var getDefendantsAndChargesTask = _mdsClient.GetDefendantAndChargesAsync(arg, cancellationToken);
        var getPcdRequestTask = _mdsClient.GetPcdRequestsAsync(arg, cancellationToken);

        await Task.WhenAll(getCaseSummaryTask, getDefendantsAndChargesTask, witnessesTask, getPcdRequestTask);

        var summarynew = getCaseSummaryTask.Result;
        var summary = new CaseSummaryDto()
        {
           Urn = summarynew.Urn,
           LeadDefendantFirstNames = summarynew.LeadDefendantFirstNames,
           LeadDefendantSurname = summarynew.LeadDefendantSurname,
           NumberOfDefendants = summarynew.NumberOfDefendants,
        }; 
        var defendantsAndCharges = getDefendantsAndChargesTask.Result.DefendantsAndCharges;
        var witnesses = MapWitnesses(witnessesTask.Result);
        var preChargeDecisionRequests = getPcdRequestTask.Result;

        return new CaseDetailsDto
        {
            Summary = summary,
            DefendantsAndCharges = defendantsAndCharges,
            Witnesses = witnesses,
            PreChargeDecisionRequests = preChargeDecisionRequests,
        };
    }

    private IEnumerable<WitnessDto> MapWitnesses(IEnumerable<BaseCaseWitnessResponse> witnesses) =>
        _caseDetailsMapper.MapWitnesses(witnesses);


}