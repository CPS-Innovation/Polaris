

using System;
using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.Factories
{
    public class CaseDataArgFactory : ICaseDataArgFactory
    {
        public UrnArg CreateUrnArg(string onBehalfOfToken, string cmsAuthValues, Guid correlationId, string urn)
        {
            return new UrnArg
            {
                OnBehalfOfToken = onBehalfOfToken,
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public CaseArg CreateCaseArg(string onBehalfOfToken, string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new CaseArg
            {
                OnBehalfOfToken = onBehalfOfToken,
                CmsAuthValues = cmsAuthValues,
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
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }
    }
}