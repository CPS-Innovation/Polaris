using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Extensions;
using Cps.MasterDataService.Infrastructure.ApiClient;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

    public async Task<Common.Dto.Response.Case.CaseDto> GetCase(MdsCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseDetails(arg);
        return _caseDetailsMapper.MapCaseDetails(@case);
    }

    public async Task<IEnumerable<Common.Dto.Response.Case.CaseDto>> GetCases(MdsUrnArgDto arg)
    {
        var caseIdentifiers = await _mdsClient.ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseDetails(_mdsArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
    }

    private async Task<CaseDetailsDto> GetCaseDetails(MdsCaseIdentifiersArgDto arg)
    {
        var request = new GetCaseSummaryRequest(arg.CaseId, arg.CorrelationId);

        var cmsAuthValuesSer = JsonConvert.DeserializeObject<AuthenticationResponse>(arg.CmsAuthValues);

        var cmsAuthValues = new CmsAuthValues(
            cmsAuthValuesSer.Cookies,
            cmsAuthValuesSer.Token,   
            arg.CorrelationId);

        var getCaseSummaryTaskNew = _masterDataServiceClient.GetCaseSummaryAsync(request, cmsAuthValues);

        var getCaseSummaryTask = _mdsClient.GetCaseSummaryAsync(_mdsArgFactory.CreateCaseIdArg(arg.CmsAuthValues, arg.CorrelationId, arg.CaseId, arg.Urn));
        var getDefendantsAndChargesTask = _mdsClient.GetDefendantAndChargesAsync(arg);
        var witnessesTask = _mdsClient.GetWitnessesAsync(arg);
        var getPcdRequestTask = _mdsClient.GetPcdRequestsAsync(arg);

        await Task.WhenAll(getCaseSummaryTask, getDefendantsAndChargesTask, witnessesTask, getPcdRequestTask, getCaseSummaryTaskNew);

        var summarynew = getCaseSummaryTaskNew.Result;
        var summary = new CaseSummaryDto()
        {
           Urn = summarynew.Urn,
           LeadDefendantFirstNames = summarynew.LeadDefendantFirstNames,
           LeadDefendantSurname = summarynew.LeadDefendantSurname,
           NumberOfDefendants = summarynew.NumberOfDefendants,
           OwningUnit = summarynew.UnitName,
           Id = summarynew.CaseId,
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