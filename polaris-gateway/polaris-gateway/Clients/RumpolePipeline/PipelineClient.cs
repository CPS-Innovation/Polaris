using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Factories;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Clients.PolarisPipeline
{
    public class PipelineClient : IPipelineClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<PipelineClient> _logger;

        public PipelineClient(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<PipelineClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
        }

        public async Task TriggerCoordinatorAsync(string caseUrn, int caseId, string accessToken, string cmsAuthValues, bool force, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(TriggerCoordinatorAsync), $"CaseId: {caseId}, Force?: {force}");
            var forceQuery = force ? "&&force=true" : string.Empty;
            _logger.LogMethodExit(correlationId, nameof(TriggerCoordinatorAsync), string.Empty);
            await SendGetRequestAsync($"cases/{caseUrn}/{caseId}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}{forceQuery}", accessToken, cmsAuthValues, correlationId);
        }

        public async Task<Tracker> GetTrackerAsync(string caseUrn, int caseId, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Acquiring the tracker for caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                response = await SendGetRequestAsync($"cases/{caseUrn}/{caseId}/tracker?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}", accessToken, correlationId);
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

            _logger.LogMethodExit(correlationId, nameof(GetTrackerAsync), $"Tracker details: {stringContent}");
            return _jsonConvertWrapper.DeserializeObject<Tracker>(stringContent, correlationId);
        }

        private async Task<HttpResponseMessage> SendGetRequestAsync(string requestUri, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SendGetRequestAsync), requestUri);

            var request = _pipelineClientRequestFactory.CreateGet(requestUri, accessToken, correlationId);
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            _logger.LogMethodExit(correlationId, nameof(SendGetRequestAsync), string.Empty);
            return response;
        }

        private async Task<HttpResponseMessage> SendGetRequestAsync(string requestUri, string accessToken, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SendGetRequestAsync), requestUri);

            var request = _pipelineClientRequestFactory.CreateGet(requestUri, accessToken, cmsAuthValues, correlationId);
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            _logger.LogMethodExit(correlationId, nameof(SendGetRequestAsync), string.Empty);
            return response;
        }
    }
}

