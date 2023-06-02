using Common.Dto.Case;
using Common.Dto.Document;
using Ddei.Domain.CaseData.Args;

namespace DdeiClient.Services.Contracts
{
    public interface IDdeiClient
    {
        Task<string> GetCmsModernToken(DdeiCmsCaseDataArgDto arg);
        Task<IEnumerable<CaseDto>> ListCases(DdeiCmsUrnArgDto arg);
        Task<CaseDto> GetCase(DdeiCmsCaseArgDto arg);
        Task<DocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId);
        Task<HttpResponseMessage> CheckoutDocument(DdeiCmsDocumentArgDto arg);
        Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg);
        Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream);
    }
}