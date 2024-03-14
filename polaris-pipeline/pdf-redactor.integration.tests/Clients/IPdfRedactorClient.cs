using Common.Dto.Request;

namespace pdf_redactor.Clients.PdfRedactor
{
  public interface IPdfRedactorClient
  {
    Task<Stream> RedactPdfAsync(RedactPdfRequestWithDocumentDto redactPdfRequest);
  }
}

