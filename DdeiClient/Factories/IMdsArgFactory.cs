using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories
{
    public interface IMdsArgFactory
    {
        CmsBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId);
        MdsCaseIdOnlyArgDto CreateCaseIdArg(string cmsAuthValues, Guid correlationId, int caseId, string urn = null);
        MdsUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
        MdsCaseIdentifiersArgDto CreateCaseIdentifiersArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);
        MdsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId);
        MdsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, string pcdId);
        MdsCaseIdentifiersArgDto CreateCaseArgFromUrnArg(MdsUrnArgDto arg, int caseId);
        MdsDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId);
        MdsDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId);
        MdsMaterialIdAndDocumentIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, long documentId);
        MdsMaterialIdAndDocumentIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId);
        MdsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, string text);
        MdsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, string text);
        MdsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, string documentName);
        MdsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, string documentName);
        MdsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long materialId, ReclassifyDocumentDto dto);
        MdsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, ReclassifyDocumentDto dto);
        MdsWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId);
    }
}

