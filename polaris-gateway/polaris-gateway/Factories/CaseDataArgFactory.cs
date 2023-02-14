

using System;
using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.Factories
{
    public class CaseDataArgFactory : ICaseDataArgFactory
    {
        public UrnArg CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new UrnArg
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public CaseArg CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new CaseArg
            {
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
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }
    }
}