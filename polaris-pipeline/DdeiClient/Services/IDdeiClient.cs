using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using Ddei.Domain.CaseData.Args;

namespace DdeiClient.Services
{
	public interface IDdeiClient
	{
		Task VerifyCmsAuthAsync(DdeiCmsCaseDataArgDto arg);
		Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCmsCaseIdArgDto arg);
		Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiCmsUrnArgDto arg);
		Task<CaseDto> GetCaseAsync(DdeiCmsCaseArgDto arg);
		Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequests(DdeiCmsCaseArgDto arg);
		Task<PcdRequestDto> GetPcdRequest(DdeiCmsPcdArgDto arg);
		Task<IEnumerable<DefendantAndChargesDto>> GetDefendantAndCharges(DdeiCmsCaseArgDto arg);
		Task<CmsDocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);
		Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId);
		Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiCmsDocumentArgDto arg);
		Task CancelCheckoutDocumentAsync(DdeiCmsDocumentArgDto arg);
		Task<HttpResponseMessage> UploadPdfAsync(DdeiCmsDocumentArgDto arg, Stream stream);
		Task<IEnumerable<DocumentNoteDto>> GetDocumentNotes(DdeiCmsDocumentNotesArgDto arg);
		Task<DocumentNoteResult> AddDocumentNote(DdeiCmsAddDocumentNoteArgDto arg);
		Task<DocumentRenamedResult> RenameDocumentAsync(DdeiCmsRenameDocumentArgDto arg);
		Task<DocumentReclassifiedResult> ReclassifyDocumentAsync(DdeiCmsReclassifyDocumentArgDto arg);
		Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducers(DdeiCmsCaseArgDto arg);
		Task<IEnumerable<CaseWitnessDto>> GetWitnesses(DdeiCmsCaseArgDto arg);
		Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiCmsCaseDataArgDto arg);
		Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiCmsWitnessStatementsArgDto arg);
		Task ReorderStatements(DdeiCmsReorderStatementsArgDto arg);
	}
}