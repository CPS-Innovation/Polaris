using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients.Contracts
{
    public interface IPdfGeneratorClient
    {
        Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId);

        Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType);
    }
}
