using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Clients
{
    public class RedactionClient : IRedactionClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<RedactionClient> _logger;

        public RedactionClient(IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<RedactionClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RedactPdfAsync), redactPdfRequest.ToJson());

            HttpResponseMessage response;
            try
            {
                var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest, correlationId), Encoding.UTF8, "application/json");
                response = await SendPutRequestAsync($"redactPdf?code={_configuration[PipelineSettings.PipelineRedactPdfFunctionAppKey]}", requestMessage, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var stringContent = await response.Content.ReadAsStringAsync();

            _logger.LogMethodExit(correlationId, nameof(RedactPdfAsync), stringContent);
            return _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent, correlationId);
        }

        private async Task<HttpResponseMessage> SendPutRequestAsync(string requestUri, HttpContent requestMessage, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SendPutRequestAsync), requestUri);

            var request = _pipelineClientRequestFactory.Create(HttpMethod.Put, requestUri, correlationId);
            request.Content = requestMessage;
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            _logger.LogMethodExit(correlationId, nameof(SendPutRequestAsync), string.Empty);
            return response;
        }
    }
}
