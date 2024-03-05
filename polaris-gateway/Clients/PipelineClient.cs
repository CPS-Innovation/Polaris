using System.Net;
using System.Text;
using Common.Configuration;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using Common.Dto.Tracker;
using Common.Factories.Contracts;
using Common.Streaming;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Dto.Case;

namespace PolarisGateway.Clients
{
    public class PipelineClient : IPipelineClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;
        private readonly ILogger<PipelineClient> _logger;

        public PipelineClient(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory,
            ILogger<PipelineClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory;
            _logger = logger;
        }

        public async Task<IList<CaseDto>> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasesPath(caseUrn)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Get, url, cmsAuthValues, correlationId);
            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<IList<CaseDto>>(stringContent);
        }

        public async Task<CaseDto> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Get, url, cmsAuthValues, correlationId);
            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<CaseDto>(stringContent);
        }

        public async Task<HttpStatusCode> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            try
            {
                var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null);
                return response.StatusCode;
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.Locked)
                {
                    return HttpStatusCode.Locked;
                }
                throw;
            }
        }

        public async Task DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId, null);
        }

        public async Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            var url = $"{RestApi.GetCaseTrackerPath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<TrackerDto>(stringContent);
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            // do not dispose of the response here
            var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
            return await _httpResponseMessageStreamFactory.Create(response);
        }

        public async Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            HttpResponseMessage response = default;
            try
            {
                response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null);
                return new OkResult();
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.Conflict)
                {
                    var lockingUser = await response?.Content?.ReadAsStringAsync();
                    return new ConflictObjectResult(lockingUser);
                }
                throw;
            }
        }

        public async Task CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId);
        }

        public async Task SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            await SendRequestAsync(HttpMethod.Put, url, cmsAuthValues, correlationId,
                new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json"));
        }

        public async Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            var url = $"{RestApi.GetCaseSearchPath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}&query={searchTerm}";
            var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(stringContent);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, string cmsAuthValues, Guid correlationId, HttpContent content = null)
        {
            var request = _pipelineClientRequestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}

