using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories.Contracts
{
    public interface IDdeiArgFactory
    {
        DdeiCmsUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);

        DdeiCmsCaseArgDto CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);

        DdeiCmsCaseArgDto CreateCaseArgFromUrnArg(DdeiCmsUrnArgDto arg, int caseId);

        DdeiCmsDocumentArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            string documentCategory,
            int documentId,
            long versionId);
    }
}

