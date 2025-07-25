using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Extensions;
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
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ICaseDetailsMapper _caseDetailsMapper;

    public DdeiCaseOrchestrationService(
            IMdsClient mdsClient,
            IDdeiArgFactory ddeiArgFactory,
            ICaseDetailsMapper caseDetailsMapper
        )
    {
        _mdsClient = mdsClient.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _caseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
    }

    public async Task<CaseDto> GetCase(DdeiCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseDetails(arg);
        return _caseDetailsMapper.MapCaseDetails(@case);
    }

    public async Task<IEnumerable<CaseDto>> GetCases(DdeiUrnArgDto arg)
    {
        var caseIdentifiers = await _mdsClient.ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseDetails(_ddeiArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
    }

    private async Task<CaseDetailsDto> GetCaseDetails(DdeiCaseIdentifiersArgDto arg)
    {
        var getCaseSummaryTask = _mdsClient.GetCaseSummaryAsync(_ddeiArgFactory.CreateCaseIdArg(arg.CmsAuthValues, arg.CorrelationId, arg.CaseId, arg.Urn));
        var getDefendantsAndChargesTask = _mdsClient.GetDefendantAndChargesAsync(arg);
        var witnessesTask = _mdsClient.GetWitnessesAsync(arg);
        var getPcdRequestTask = _mdsClient.GetPcdRequestsAsync(arg);

        await Task.WhenAll(getCaseSummaryTask, getDefendantsAndChargesTask, witnessesTask, getPcdRequestTask);

        var summary = getCaseSummaryTask.Result;
        var defendantsAndCharges = getDefendantsAndChargesTask.Result.DefendantsAndCharges;
        var witnesses = MapWitnesses(witnessesTask.Result);
        var preChargeDecisionRequests = getPcdRequestTask.Result;

        return new CaseDetailsDto
        {
            Summary = summary,
            DefendantsAndCharges = defendantsAndCharges,
            Witnesses = witnesses,
            PreChargeDecisionRequests = preChargeDecisionRequests
        };
    }

    private IEnumerable<WitnessDto> MapWitnesses(IEnumerable<BaseCaseWitnessResponse> witnesses) =>
        _caseDetailsMapper.MapWitnesses(witnesses);


}