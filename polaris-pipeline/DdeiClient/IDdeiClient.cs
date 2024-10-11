using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei
{
	public interface IDdeiClient
	{
		Task VerifyCmsAuthAsync(DdeiBaseArgDto arg);
		Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCaseIdOnlyArgDto arg);
		Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArgDto arg);
		Task<CaseDto> GetCaseAsync(DdeiCaseIdentifiersArgDto arg);
		Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequests(DdeiCaseIdentifiersArgDto arg);
		Task<PcdRequestDto> GetPcdRequest(DdeiCmsPcdArgDto arg);
		Task<IEnumerable<DefendantAndChargesDto>> GetDefendantAndCharges(DdeiCaseIdentifiersArgDto arg);
		Task<CmsDocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);
		Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId);
		Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiCmsDocumentIdAndVersionIdArgDto arg);
		Task CancelCheckoutDocumentAsync(DdeiCmsDocumentIdAndVersionIdArgDto arg);
		Task<HttpResponseMessage> UploadPdfAsync(DdeiCmsDocumentIdAndVersionIdArgDto arg, Stream stream);
		Task<IEnumerable<DocumentNoteDto>> GetDocumentNotes(DdeiCmsDocumentNotesArgDto arg);
		Task<DocumentNoteResult> AddDocumentNote(DdeiCmsAddDocumentNoteArgDto arg);
		Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiCmsRenameDocumentArgDto arg);
		Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiCmsReclassifyDocumentArgDto arg);
		Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducers(DdeiCaseIdentifiersArgDto arg);
		Task<IEnumerable<CaseWitnessDto>> GetWitnesses(DdeiCaseIdentifiersArgDto arg);
		Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg);
		Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiCmsWitnessStatementsArgDto arg);
	}
}