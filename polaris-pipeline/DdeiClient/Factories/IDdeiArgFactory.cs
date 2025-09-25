using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Factories
{
    public interface IDdeiArgFactory
    {
        DdeiBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId);
        DdeiCaseIdOnlyArgDto CreateCaseIdArg(string cmsAuthValues, Guid correlationId, int caseId, string urn = null);
        DdeiUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
        DdeiCaseIdentifiersArgDto CreateCaseIdentifiersArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);
        DdeiPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId);
        DdeiPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, string pcdId);
        DdeiCaseIdentifiersArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId);
        DdeiDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId);
        DdeiDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId);
        DdeiDocumentIdAndVersionIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, long versionId);
        DdeiDocumentIdAndVersionIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId);
        DdeiAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text);
        DdeiAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, string text);
        DdeiRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName);
        DdeiRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, string documentName);
        DdeiReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto);
        DdeiReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, ReclassifyDocumentDto dto);
        DdeiWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId);
    }
}

