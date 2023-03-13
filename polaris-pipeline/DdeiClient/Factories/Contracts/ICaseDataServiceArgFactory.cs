using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories.Contracts
{
    public interface ICaseDataArgFactory
    {
        CmsUrnArg CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);

        CmsCaseArg CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);

        CmsCaseArg CreateCaseArgFromUrnArg(CmsUrnArg arg, int caseId);
    }
}

