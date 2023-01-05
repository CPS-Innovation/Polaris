

using System;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.Factories
{
    public class CaseDataArgFactory : ICaseDataArgFactory
    {
        public UrnArg CreateUrnArg(string onBehalfOfToken, string upstreamToken, Guid correlationId, string urn)
        {
            return new UrnArg
            {
                OnBehalfOfToken = onBehalfOfToken,
                UpstreamToken = upstreamToken,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public CaseArg CreateCaseArg(string onBehalfOfToken, string upstreamToken, Guid correlationId, string urn, int caseId)
        {
            return new CaseArg
            {
                OnBehalfOfToken = onBehalfOfToken,
                UpstreamToken = upstreamToken,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId
            };
        }

        public CaseArg CreateCaseArgFromUrnArg(UrnArg arg, int caseId)
        {
            return new CaseArg
            {
                OnBehalfOfToken = arg.OnBehalfOfToken,
                UpstreamToken = arg.UpstreamToken,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }
    }
}