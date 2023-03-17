using Ddei.Domain.CaseData.Args;
using Ddei.Factories.Contracts;

namespace Ddei.Factories
{
    public class CaseDataArgFactory : ICaseDataArgFactory
    {
        public CmsUrnArg CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new CmsUrnArg
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public CmsCaseArg CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new CmsCaseArg
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId
            };
        }

        public CmsCaseArg CreateCaseArgFromUrnArg(CmsUrnArg arg, int caseId)
        {
            return new CmsCaseArg
            {
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }
    }
}