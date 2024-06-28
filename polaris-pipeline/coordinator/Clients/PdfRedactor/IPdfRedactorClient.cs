using Common.Dto.Request;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients.PdfRedactor
{
    public interface IPdfRedactorClient
    {
        Task<Stream> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId);
        Task<Stream> RemoveDocumentPages(string caseUrn, string caseId, string documentId, RemoveDocumentPagesWithDocumentDto removeDocumentPagesDto, Guid correlationId);
    }
}
