using polaris_common.Domain.Document;
using polaris_common.Dto.Request;
using polaris_common.Dto.Response;

namespace polaris_common.Clients.Contracts
{
    public interface IPdfGeneratorClient
    {
        Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId);

        Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType);
    }
}
