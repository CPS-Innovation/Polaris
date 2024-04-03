using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients.PdfRedactor
{
  public interface IPdfRedactorClient
  {
    Task<RedactPdfResponse> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    Task<Stream> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId);
  }
}
