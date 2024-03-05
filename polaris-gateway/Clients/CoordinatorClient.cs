
using Common.Configuration;
using Common.Dto.Request;
using Common.Factories.Contracts;
using Common.Streaming;
using Common.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace PolarisGateway.Clients
{
    public class CoordinatorClient : ICoordinatorClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

        public CoordinatorClient(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory;
        }

        public async Task<HttpResponseMessage> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCasesPath(caseUrn),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCasePath(caseUrn, caseId),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetCasePath(caseUrn, caseId),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetCasePath(caseUrn, caseId),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseTrackerPath(caseUrn, caseId),
                null,
                correlationId);
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            // do not dispose of the response here
            var response = await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId),
                null,
                correlationId);
            return await _httpResponseMessageStreamFactory.Create(response);
        }

        public async Task<HttpResponseMessage> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, polarisDocumentId),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, polarisDocumentId),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId),
                cmsAuthValues,
                correlationId);
        }

        public async Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseSearchQueryPath(caseUrn, caseId, searchTerm),
                null,
                correlationId);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, string cmsAuthValues, Guid correlationId, HttpContent content = null)
        {
            var request = _pipelineClientRequestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues);
            request.Content = content;
            request.Headers.Add("x-functions-key", _configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]);
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}

