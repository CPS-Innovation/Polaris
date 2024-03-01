using System.Net;
using System.Text;
using Common.Configuration;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Factories.Contracts;
using Common.Streaming;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using PolarisGateway;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Dto.Case;

namespace Gateway.Clients
{
    public class PipelineClient : IPipelineClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;
        private readonly ILogger<PipelineClient> _logger;
        private static readonly HttpStatusCode[] ExpectedRefreshErrorStatusCodes = { HttpStatusCode.Locked };
        private static readonly HttpStatusCode[] ExpectedCheckoutErrorStatusCodes = { HttpStatusCode.Conflict };

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

        public async Task<IEnumerable<CaseDto>> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasesPath(caseUrn)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId);
            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<IEnumerable<CaseDto>>(stringContent);
        }

        public async Task<CaseDto> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId);
            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<CaseDto>(stringContent);
        }

        public async Task<HttpStatusCode> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null, ExpectedRefreshErrorStatusCodes);
            return response.StatusCode;
        }

        public async Task<IActionResult> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId, null, ExpectedRefreshErrorStatusCodes);
            HttpStatusCode statusCode = response.StatusCode;

            return new StatusCodeResult((int)statusCode);
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
            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null, ExpectedCheckoutErrorStatusCodes);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

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

        public async Task CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId);
        }

        public async Task<RedactPdfResponse> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            var content = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json");
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}";
            using var response = await SendRequestAsync(HttpMethod.Put, url, cmsAuthValues, correlationId, content);
            var stringContent = await response.Content.ReadAsStringAsync();

            var result = _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent);

            if (!result.Succeeded)
            {
                // todo: remove returning a Succeeded boolean from redaction endpoint,
                //   endpoint should return unhappy status code
                throw new Exception("Error Saving redaction details");
            }

            return result;
        }

        public async Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetCaseSearchPath(caseUrn, caseId)}?code={_configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]}&query={searchTerm}";
                response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
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

            return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(stringContent);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, string cmsAuthValues, Guid correlationId, HttpContent content = null, HttpStatusCode[] expectedResponseCodes = null)
        {
            var request = _pipelineClientRequestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (expectedResponseCodes?.Contains(response.StatusCode) != true)
                response.EnsureSuccessStatusCode();

            return response;
        }
    }
}

