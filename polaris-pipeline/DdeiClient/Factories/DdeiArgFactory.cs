using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories
{
    public class DdeiArgFactory : IDdeiArgFactory
    {
        public DdeiArgFactory()
        {
        }

        public DdeiBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId)
        {
            return new DdeiBaseArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            };
        }

        public DdeiCaseIdOnlyArgDto CreateCaseIdArg(string cmsAuthValues, Guid correlationId, int caseId)
        {
            return new DdeiCaseIdOnlyArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                CaseId = caseId
            };
        }

        public DdeiUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new DdeiUrnArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public DdeiCaseIdentifiersArgDto CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new DdeiCaseIdentifiersArgDto
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

        public DdeiCaseIdentifiersArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId)
        {
            return new DdeiCaseIdentifiersArgDto
            {
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }

        public DdeiCmsDocumentIdAndVersionIdArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            long documentId,
            long versionId)
        {
            return new DdeiCmsDocumentIdAndVersionIdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                VersionId = versionId
            };
        }

        public DdeiCmsDocumentNotesArgDto CreateDocumentNotesArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId)
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

        public DdeiCmsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text)
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

        public DdeiCmsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName)
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

        public DdeiCmsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto)
        {
            return new DdeiCmsReclassifyDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                DocumentTypeId = dto.DocumentTypeId,
                Exhibit = dto.Exhibit,
                Statement = dto.Statement,
                Other = dto.Other,
                Immediate = dto.Immediate
            };
        }

        public DdeiCmsWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId)
        {
            return new DdeiCmsWitnessStatementsArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                WitnessId = witnessId
            };
        }
    }
}