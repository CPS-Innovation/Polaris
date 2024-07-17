using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories
{
    public class DdeiArgFactory : IDdeiArgFactory
    {
        public DdeiCmsCaseDataArgDto CreateCmsAuthValuesArg(string partialCmsAuthValues, Guid correlationId)
        {
            return new DdeiCmsCaseDataArgDto
            {
                CorrelationId = correlationId,
                CmsAuthValues = partialCmsAuthValues
            };
        }
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

        public DdeiCmsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId)
        {
            return new DdeiCmsPcdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                PcdId = pcdId
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
            int documentId,
            long versionId)
        {
            return new DdeiCmsDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                VersionId = versionId
            };
        }

        public DdeiCmsDocumentNotesArgDto CreateDocumentNotesArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
        {
            return new DdeiCmsDocumentNotesArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId
            };
        }

        public DdeiCmsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int documentId, string text)
        {
            return new DdeiCmsAddDocumentNoteArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                Text = text
            };
        }

        public DdeiCmsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int documentId, string documentName)
        {
            return new DdeiCmsRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                DocumentName = documentName
            };
        }
    }
}