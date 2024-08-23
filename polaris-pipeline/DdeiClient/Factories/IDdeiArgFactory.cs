using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories
{
    public interface IDdeiArgFactory
    {
        DdeiCmsCaseDataArgDto CreateCmsAuthValuesArg(string partialCmsAuthValues, Guid correlationId);
        DdeiCmsUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
        DdeiCmsCaseArgDto CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);
        DdeiCmsPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId);
        DdeiCmsCaseArgDto CreateCaseArgFromUrnArg(DdeiCmsUrnArgDto arg, int caseId);
        DdeiCmsDocumentArgDto CreateDocumentArgDto(
            string cmsAuthValues,
            Guid correlationId,
            string urn,
            int caseId,
            int documentId,
            long versionId);
        DdeiCmsDocumentNotesArgDto CreateDocumentNotesArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId);
        DdeiCmsAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int documentId, string text);
        DdeiCmsRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int documentId, string documentName);
        DdeiCmsReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int documentId, ReclassifyDocumentDto dto);
        DdeiCmsCaseDataArgDto CreateMaterialTypeListArgDto(string cmsAuthValues, Guid correlationId);
    }
}

