using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;
using Common.Streaming;
using Microsoft.Extensions.Configuration;

namespace coordinator.Clients
{
  public class PdfRedactorClient : IPdfRedactorClient
  {
    private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public PdfRedactorClient(IPipelineClientRequestFactory pipelineClientRequestFactory,
        HttpClient httpClient,
        IConfiguration configuration,
        IJsonConvertWrapper jsonConvertWrapper)
    {
      _pipelineClientRequestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    }

    public async Task<RedactPdfResponse> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
    {
      HttpResponseMessage response;
      try
      {
        var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json");

        var request = _pipelineClientRequestFactory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(caseUrn, caseId, documentId)}?code={_configuration[Constants.ConfigKeys.PipelineRedactorPdfFunctionAppKey]}", correlationId);
        request.Content = requestMessage;

        response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
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

      var stringContent = await response.Content.ReadAsStringAsync();
      return _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent);
    }
  }
}
