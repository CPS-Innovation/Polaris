using System.Text;
using System.Text.Json;
using Common.Configuration;
using Common.Dto.Request;
using coordinator.Clients.PdfRedactor;

namespace pdf_redactor.integration.tests.Clients
{
  public class PdfRedactorClient : IPdfRedactorClient
  {
    private readonly HttpClient _httpClient;
    private readonly IRequestFactory _requestFactory;

    public PdfRedactorClient(HttpClient httpClient, IRequestFactory requestFactory)
    {
      _httpClient = httpClient;
      _requestFactory = requestFactory;
    }

    public async Task<Stream> RedactPdfAsync(RedactPdfRequestWithDocumentDto redactPdfRequest)
    {
      Guid currentCorrelationId = Guid.NewGuid();

      var requestMessage = new StringContent(JsonSerializer.Serialize(redactPdfRequest), Encoding.UTF8, "application/json");

      var redactRequest = _requestFactory.Create(HttpMethod.Put, $"/api/{RestApi.GetRedactPdfPath("pdf-redactor-integration-tests-urn", "1234", "pdf-redactor-integration-tests-documentId")}", currentCorrelationId);
      redactRequest.Content = requestMessage;

      var pdfStream = new MemoryStream();
      using var response = await _httpClient.SendAsync(redactRequest, HttpCompletionOption.ResponseHeadersRead);

      response.EnsureSuccessStatusCode();
      await response.Content.CopyToAsync(pdfStream);
      pdfStream.Seek(0, SeekOrigin.Begin);

      return pdfStream;
    }
  }
}