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

        public CmsBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId)
        {
            return new CmsBaseArgDto
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

        public MdsDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId)
        {
            return new MdsDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                MaterialId = materialId
            };
        }

        public MdsDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId)
            => CreateDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(materialId));

        public MdsMaterialIdAndDocumentIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, long versionId)
        {
            return new MdsMaterialIdAndDocumentIdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                MaterialId = materialId,
                DocumentId = versionId,
            };
        }

        public MdsMaterialIdAndDocumentIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId)
            => CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(materialId), documentId);

        public MdsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, string text)
        {
            return new MdsAddDocumentNoteArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                MaterialId = materialId,
                Text = text,
            };
        }

        public MdsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, string text)
            => CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(materialId), text);

        public MdsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, string documentName)
        {
            return new MdsRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                MaterialId = materialId,
                DocumentName = documentName
            };
        }

        public MdsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, string documentName)
            =>
            CreateRenameDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(materialId), documentName);

        public MdsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, ReclassifyDocumentDto dto)
        {
            return new MdsReclassifyDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                MaterialId = materialId,
                DocumentTypeId = dto.DocumentTypeId,
                Exhibit = dto.Exhibit,
                Statement = dto.Statement,
                Other = dto.Other,
                Immediate = dto.Immediate,
            };
        }

        public MdsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, ReclassifyDocumentDto dto)
            => CreateReclassifyDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(materialId), dto);

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