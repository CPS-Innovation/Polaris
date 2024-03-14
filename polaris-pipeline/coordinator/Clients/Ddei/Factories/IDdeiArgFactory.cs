using System;
using coordinator.Clients.Ddei.Domain.CaseData.Args;

namespace coordinator.Clients.Ddei.Factories
{
    public interface IDdeiArgFactory
    {
        DdeiBaseArg CreateCmsAuthValuesArg(string partialCmsAuthValues, Guid correlationId);
        DdeiUrnArg CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
        DdeiCaseIdArg CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);
        DdeiCaseIdArg CreateCaseArgFromUrnArg(DdeiUrnArg arg, int caseId);
        DdeiDocumentArg CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            string documentCategory,
            int documentId,
            long versionId);
    }
}

