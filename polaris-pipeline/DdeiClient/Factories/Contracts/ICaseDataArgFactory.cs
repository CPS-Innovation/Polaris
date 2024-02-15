using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories.Contracts
{
    public interface ICaseDataArgFactory
    {
        DdeiCmsCaseDataArgDto CreateCmsAuthValuesArg(string partialCmsAuthValues, Guid correlationId);
        DdeiCmsUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
        DdeiCmsCaseArgDto CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);
        DdeiCmsCaseArgDto CreateCaseArgFromUrnArg(DdeiCmsUrnArgDto arg, int caseId);
    }
}

