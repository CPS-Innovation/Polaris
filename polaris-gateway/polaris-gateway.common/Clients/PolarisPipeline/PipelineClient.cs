using System.Net;
using System.Text;
using Common.Configuration;
using Common.Constants;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Factories.Contracts;
using Common.Streaming;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gateway.Clients.PolarisPipeline
{
    public class PipelineClient : IPipelineClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;
        private readonly ILogger<PipelineClient> _logger;
        private static readonly HttpStatusCode[] ExpectedRefreshErrorStatusCodes = { HttpStatusCode.Locked };
        private static readonly HttpStatusCode[] ExpectedCheckoutErrorStatusCodes = { HttpStatusCode.Conflict };

        public PipelineClient(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory,
            ILogger<PipelineClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<StatusCodeResult> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
            using var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null, ExpectedRefreshErrorStatusCodes);

            return new StatusCodeResult((int)response.StatusCode);
        }

        public async Task<IActionResult> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
            using var response = await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId, null, ExpectedRefreshErrorStatusCodes);

            return new StatusCodeResult((int)response.StatusCode);
        }

        public async Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            try
            {
                var url = $"{RestApi.GetCaseTrackerPath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                using var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
                var stringContent = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<TrackerDto>(stringContent, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {

            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                // do not dispose of the response here
                var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
                return await _httpResponseMessageStreamFactory.Create(response);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // todo: clear this up as part of failure-flow work  
                    return null;
                }
                throw;
            }
        }

        public async Task<string> GenerateDocumentSasUrlAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            try
            {
                var url = $"{RestApi.GetDocumentSasPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                using var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                using var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null, ExpectedCheckoutErrorStatusCodes);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return new OkResult();

                    case HttpStatusCode.Conflict:
                        var lockingUser = await response.Content.ReadAsStringAsync();
                        return new ConflictObjectResult(lockingUser);

                    default:
                        return new StatusCodeResult(500);
                };
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<IActionResult> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId);
                return new OkResult();
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<RedactPdfResponse> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            var content = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest, correlationId), Encoding.UTF8, "application/json");
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
            using var response = await SendRequestAsync(HttpMethod.Put, url, cmsAuthValues, correlationId, content);
            var stringContent = await response.Content.ReadAsStringAsync();

            return _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent, correlationId);
        }

        public async Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            try
            {
                var url = $"{RestApi.GetCaseSearchPath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}&query={searchTerm}";
                using var response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
                var stringContent = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(stringContent, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, string cmsAuthValues, Guid correlationId, HttpContent content = null, HttpStatusCode[] expectedResponseCodes = null)
        {
            var request = _pipelineClientRequestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues);
            request.Content = content;

            var httpClientName = requestUri.StartsWith("urns") ? nameof(PipelineClient) : $"Lowlevel{nameof(PipelineClient)}";

            var httpClient = _httpClientFactory.CreateClient(httpClientName);
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (expectedResponseCodes?.Contains(response.StatusCode) != true)
                response.EnsureSuccessStatusCode();

            return response;
        }
    }
}