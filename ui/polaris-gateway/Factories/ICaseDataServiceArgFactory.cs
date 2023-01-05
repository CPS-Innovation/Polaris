using System;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.Factories
{
    public interface ICaseDataArgFactory
    {
        UrnArg CreateUrnArg(string onBehalfOfToken, string upstreamToken, Guid correlationId, string urn);

        CaseArg CreateCaseArg(string onBehalfOfToken, string upstreamToken, Guid correlationId, string urn, int caseId);

        CaseArg CreateCaseArgFromUrnArg(UrnArg arg, int caseId);
    }
}

