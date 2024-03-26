using Common.Dto.Request;

namespace pdf_redactor.integration.tests.Clients
{
  public interface IPdfRedactorClient
  {
    Task<Stream> RedactPdfAsync(RedactPdfRequestWithDocumentDto redactPdfRequest);
  }
}

