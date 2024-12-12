using Common.Dto.Request;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients.PdfRedactor
{
    public interface IPdfRedactorClient
    {
        Task<Stream> RedactPdfAsync(string caseUrn, int caseId, string documentId, long versionId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId);
        Task<Stream> ModifyDocument(string caseUrn, int caseId, string documentId, long versionId, ModifyDocumentWithDocumentDto modifyDocumentDto, Guid correlationId);
    }
}
