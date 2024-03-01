using Common.Dto.Case;
using Common.Dto.Document;
using Ddei.Domain.CaseData.Args;

namespace DdeiClient.Services
{
    public interface IDdeiClient
    {
        Task<CmsAuthValuesDto> GetFullCmsAuthValuesAsync(DdeiCmsCaseDataArgDto arg);
        Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCmsCaseIdArgDto arg);
        Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiCmsUrnArgDto arg);
        Task<CaseDto> GetCaseAsync(DdeiCmsCaseArgDto arg);
        Task<CmsDocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId);
        Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiCmsDocumentArgDto arg);
        Task CancelCheckoutDocumentAsync(DdeiCmsDocumentArgDto arg);
        Task UploadPdfAsync(DdeiCmsDocumentArgDto arg, Stream stream);
    }
}