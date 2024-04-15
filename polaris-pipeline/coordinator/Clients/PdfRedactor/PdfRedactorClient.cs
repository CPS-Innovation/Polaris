using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Wrappers;

namespace coordinator.Clients.PdfRedactor
{
  public class PdfRedactorClient : IPdfRedactorClient
  {
    private readonly IRequestFactory _pipelineClientRequestFactory;
    private readonly HttpClient _httpClient;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public PdfRedactorClient(IRequestFactory pipelineClientRequestFactory,
        HttpClient httpClient,
        IJsonConvertWrapper jsonConvertWrapper)
    {
      _pipelineClientRequestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    }

    public async Task<Stream> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId)
    {
      try
      {
        var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json");

        var request = _pipelineClientRequestFactory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(caseUrn, caseId, documentId)}", correlationId);
        request.Content = requestMessage;

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
      }
      catch (HttpRequestException exception)
      {
        if (exception.StatusCode == HttpStatusCode.NotFound)
        {
          // todo: check if ok to swallow a not found response?
          return null;
        }
        throw;
      }
    }
  }
}
