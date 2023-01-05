using System;
using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.Factories
{
    public interface ICaseDataArgFactory
    {
        UrnArg CreateUrnArg(string onBehalfOfToken, string upstreamToken, Guid correlationId, string urn);

        CaseArg CreateCaseArg(string onBehalfOfToken, string upstreamToken, Guid correlationId, string urn, int caseId);

        CaseArg CreateCaseArgFromUrnArg(UrnArg arg, int caseId);
    }
}

