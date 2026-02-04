using Common.Domain.Document;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories
{
    public class MdsArgFactory : IMdsArgFactory
    {
        public MdsArgFactory()
        {
        }

        public MdsBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId)
        {
            return new MdsBaseArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
            };
        }

        public MdsCaseIdOnlyArgDto CreateCaseIdArg(string cmsAuthValues, Guid correlationId, int caseId, string urn = null)
        {
            return new MdsCaseIdOnlyArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                CaseId = caseId,
                Urn = urn,
            };
        }

        public MdsUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new MdsUrnArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public MdsCaseIdentifiersArgDto CreateCaseIdentifiersArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new MdsCaseIdentifiersArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId
            };
        }

        public MdsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId)
        {
            return new MdsPcdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                PcdId = pcdId
            };
        }

        public MdsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, string pcdId)
            => CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiPcdId(pcdId));

        public MdsCaseIdentifiersArgDto CreateCaseArgFromUrnArg(MdsUrnArgDto arg, int caseId)
        {
            return new MdsCaseIdentifiersArgDto
            {
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId,
            };
        }

        public MdsDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId)
        {
            return new MdsDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId
            };
        }

        public MdsDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
            => CreateDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId));

        public MdsDocumentIdAndVersionIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, long versionId)
        {
            return new MdsDocumentIdAndVersionIdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                VersionId = versionId,
            };
        }

        public MdsDocumentIdAndVersionIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
            => CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), versionId);

        public MdsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text)
        {
            return new MdsAddDocumentNoteArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                Text = text,
            };
        }

        public MdsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, string text)
            => CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), text);

        public MdsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName)
        {
            return new MdsRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                DocumentName = documentName
            };
        }

        public MdsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, string documentName)
            =>
            CreateRenameDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), documentName);

        public MdsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto)
        {
            return new MdsReclassifyDocumentArgDto
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
                Immediate = dto.Immediate,
            };
        }

        public MdsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, ReclassifyDocumentDto dto)
            => CreateReclassifyDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), dto);

        public MdsWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId)
        {
            return new MdsWitnessStatementsArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                WitnessId = witnessId
            };
        }

        private static long ConvertToDdeiDocumentId(string documentId) => DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document);

        private static int ConvertToDdeiPcdId(string documentId) => (int)DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.PreChargeDecisionRequest);

    }
}