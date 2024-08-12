using System.Reflection.Metadata;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.PreCharge;

namespace DdeiClient.Services
{
	public interface IDdeiClient
	{
		Task<CmsAuthValuesDto> GetFullCmsAuthValuesAsync(DdeiCmsCaseDataArgDto arg);
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
		Task<IEnumerable<DocumentExhibitProducerDto>> GetExhibitProducers(DdeiCmsCaseArgDto arg);
	}
}