using System;
using coordinator.Clients.Ddei.Domain.CaseData.Args;

namespace coordinator.Clients.Ddei.Factories
{
    public class DdeiArgFactory : IDdeiArgFactory
    {
        public DdeiBaseArg CreateCmsAuthValuesArg(string partialCmsAuthValues, Guid correlationId)
        {
            return new DdeiBaseArg
            {
                CorrelationId = correlationId,
                CmsAuthValues = partialCmsAuthValues
            };
        }
        public DdeiUrnArg CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new DdeiUrnArg
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public DdeiCaseIdArg CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new DdeiCaseIdArg
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId
            };
        }

        public DdeiCaseIdArg CreateCaseArgFromUrnArg(DdeiUrnArg arg, int caseId)
        {
            return new DdeiCaseIdArg
            {
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }

        public DdeiDocumentArg CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            string documentCategory,
            int documentId,
            long versionId)
        {
            return new DdeiDocumentArg
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                CmsDocCategory = documentCategory,
                DocumentId = documentId,
                VersionId = versionId
            };
        }
    }
}