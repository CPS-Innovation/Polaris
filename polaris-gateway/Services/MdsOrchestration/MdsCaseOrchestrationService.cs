// -----------------------------------------------------------------------------
// <copyright file="MdsCaseOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
// <summary>
//     Contains the MdsCaseOrchestrationService implementation.
// </summary>
// -----------------------------------------------------------------------------
namespace PolarisGateway.Services.MdsOrchestration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response;
    using Common.Dto.Response.Case;
    using Ddei.Domain.CaseData.Args.Core;
    using Ddei.Factories;
    using Ddei.Mappers;
    using DdeiClient.Clients.Interfaces;

    public class MdsCaseOrchestrationService(IMdsClient mdsClient,
                                IMasterDataServiceClient masterDataServiceClient,
                                IMdsArgFactory mdsArgFactory,
                                ICaseDetailsMapper caseDetailsMapper)
                                : IMdsCaseOrchestrationService
    {
        private readonly IMdsClient mdsClient = mdsClient;
        private readonly IMasterDataServiceClient masterDataServiceClient = masterDataServiceClient;
        private readonly IMdsArgFactory mdsArgFactory = mdsArgFactory;
        private readonly ICaseDetailsMapper caseDetailsMapper = caseDetailsMapper;

        public async Task<CaseDto> GetCase(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
        {
            var @case = await this.GetCaseDetails(arg, cancellationToken);
            return this.caseDetailsMapper.MapCaseDetails(@case);
        }

        public async Task<IEnumerable<CaseDto>> GetCases(MdsUrnArgDto arg, CancellationToken cancellationToken = default)
        {
            var caseIdentifiers = await this.mdsClient.ListCaseIdsAsync(arg, cancellationToken);

            var calls = caseIdentifiers.Select(async caseIdentifier =>
                await this.GetCaseDetails(this.mdsArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id), cancellationToken));

            var cases = await Task.WhenAll(calls);
            return cases.Select(@case => this.caseDetailsMapper.MapCaseDetails(@case));
        }

        private async Task<CaseDetailsDto> GetCaseDetails(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
        {
            var caseSummaryRequest = new GetCaseSummaryRequest(arg.CaseId, arg.CorrelationId);

            var cmsAuthValues = new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId);

            var getCaseSummaryTask = this.masterDataServiceClient.GetCaseSummaryAsync(caseSummaryRequest, cmsAuthValues, cancellationToken);
            var witnessesTask = this.mdsClient.GetWitnessesAsync(arg, cancellationToken);
            var getDefendantsAndChargesTask = this.mdsClient.GetDefendantAndChargesAsync(arg, cancellationToken);
            var getPcdRequestTask = this.mdsClient.GetPcdRequestsAsync(arg, cancellationToken);

            var caseSummaryResponse = await getCaseSummaryTask;
            var summary = new CaseSummaryDto()
            {
                Id = caseSummaryResponse.CaseId,
                OwningUnit = caseSummaryResponse.UnitName,
                Urn = caseSummaryResponse.Urn,
                LeadDefendantFirstNames = caseSummaryResponse.LeadDefendantFirstNames,
                LeadDefendantSurname = caseSummaryResponse.LeadDefendantSurname,
                NumberOfDefendants = caseSummaryResponse.NumberOfDefendants,
            };
            var defendantsAndCharges = await getDefendantsAndChargesTask;
            var witnesses = this.MapWitnesses(await witnessesTask);
            var preChargeDecisionRequests = await getPcdRequestTask;

            return new CaseDetailsDto
            {
                Summary = summary,
                DefendantsAndCharges = defendantsAndCharges.DefendantsAndCharges,
                Witnesses = witnesses,
                PreChargeDecisionRequests = preChargeDecisionRequests,
            };
        }

        private IEnumerable<WitnessDto> MapWitnesses(IEnumerable<BaseCaseWitnessResponse> witnesses) =>
            this.caseDetailsMapper.MapWitnesses(witnesses);
    }
}
