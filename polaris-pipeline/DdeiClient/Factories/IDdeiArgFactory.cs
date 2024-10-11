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
        DdeiCmsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId);
        DdeiCaseIdentifiersArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId);
        DdeiCmsDocumentIdAndVersionIdArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            long documentId,
            long versionId);
        DdeiCmsDocumentNotesArgDto CreateDocumentNotesArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId);
        DdeiCmsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text);
        DdeiCmsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName);
        DdeiCmsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto);
        DdeiCmsWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId);
    }
}

