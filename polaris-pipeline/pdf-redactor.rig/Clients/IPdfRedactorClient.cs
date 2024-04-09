using Common.Dto.Request;

namespace pdf_redactor.rig.Clients
{
  public interface IPdfRedactorClient
  {
    Task<Stream> RedactPdfAsync(RedactPdfRequestWithDocumentDto redactPdfRequest);
  }
}

