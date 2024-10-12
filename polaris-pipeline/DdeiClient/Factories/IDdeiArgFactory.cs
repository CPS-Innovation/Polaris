using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories
{
    public interface IDdeiArgFactory
    {
        DdeiBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId);
        DdeiCaseIdOnlyArgDto CreateCaseIdArg(string cmsAuthValues, Guid correlationId, int caseId);
        DdeiUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
        DdeiCaseIdentifiersArgDto CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);
        DdeiPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId);
        DdeiCaseIdentifiersArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId);
        DdeiDocumentIdAndVersionIdArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            long documentId,
            long versionId);
        DdeiDocumentNotesArgDto CreateDocumentNotesArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId);
        DdeiAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text);
        DdeiRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName);
        DdeiReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto);
        DdeiWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId);
    }
}

