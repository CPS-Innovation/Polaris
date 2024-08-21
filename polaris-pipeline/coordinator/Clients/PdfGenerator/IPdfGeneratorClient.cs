using Common.Domain.Document;
using Common.Dto.Request;
using coordinator.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients.PdfGenerator
{
    public interface IPdfGeneratorClient
    {
        Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType);
        Task<Stream> GenerateThumbnail(string caseUrn, string caseId, string documentId, GenerateThumbnailWithDocumentDto thumbnailRequest, Guid correlationId);
    }
}
