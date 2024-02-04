using Ddei.Domain.CaseData.Args;
using Ddei.Factories.Contracts;

namespace Ddei.Factories
{
    public class DdeiArgFactory : IDdeiArgFactory
    {
        public DdeiCmsUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new DdeiCmsUrnArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public DdeiCmsCaseArgDto CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new DdeiCmsCaseArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId
            };
        }

        public DdeiCmsCaseArgDto CreateCaseArgFromUrnArg(DdeiCmsUrnArgDto arg, int caseId)
        {
            return new DdeiCmsCaseArgDto
            {
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }

        public DdeiCmsDocumentArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            string documentCategory,
            int documentId,
            long versionId)
        {
            return new DdeiCmsDocumentArgDto
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