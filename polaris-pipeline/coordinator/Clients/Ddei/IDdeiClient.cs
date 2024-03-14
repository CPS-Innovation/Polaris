using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Dto.Case;
using Common.Dto.Document;
using coordinator.Clients.Ddei.Domain.CaseData.Args;

namespace coordinator.Clients.Ddei
{
    public interface IDdeiClient
    {
        Task<CmsAuthValuesDto> GetFullCmsAuthValuesAsync(DdeiBaseArg arg);
        Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiGetUrnArg arg);
        Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArg arg);
        Task<CaseDto> GetCaseAsync(DdeiCaseIdArg arg);
        Task<CmsDocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId);
        Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiDocumentArg arg);
        Task CancelCheckoutDocumentAsync(DdeiDocumentArg arg);
        Task UploadPdfAsync(DdeiDocumentArg arg, Stream stream);
    }
}