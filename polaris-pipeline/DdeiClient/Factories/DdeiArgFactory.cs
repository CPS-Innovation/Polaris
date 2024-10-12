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

        public DdeiPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId)
        {
            return new DdeiPcdArgDto
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

        public DdeiDocumentIdAndVersionIdArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            long documentId,
            long versionId)
        {
            return new DdeiDocumentIdAndVersionIdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                VersionId = versionId
            };
        }

        public DdeiDocumentNotesArgDto CreateDocumentNotesArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId)
        {
            return new DdeiDocumentNotesArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId
            };
        }

        public DdeiAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text)
        {
            return new DdeiAddDocumentNoteArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                Text = text
            };
        }

        public DdeiRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName)
        {
            return new DdeiRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                DocumentName = documentName
            };
        }

        public DdeiReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto)
        {
            return new DdeiReclassifyDocumentArgDto
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

        public DdeiWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId)
        {
            return new DdeiWitnessStatementsArgDto
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