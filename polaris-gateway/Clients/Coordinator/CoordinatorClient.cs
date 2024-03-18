using System.Text;
using Common.Configuration;
using Common.Dto.Request;
using Common.ValueObjects;
using Newtonsoft.Json;

namespace PolarisGateway.Clients.Coordinator
{
    public class CoordinatorClient : ICoordinatorClient
    {
        private readonly IRequestFactory _requestFactory;
        private readonly HttpClient _httpClient;

        public CoordinatorClient(
            IRequestFactory requestFactory,
            HttpClient httpClient)
        {
            _requestFactory = requestFactory;
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCasesPath(caseUrn),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCasePath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetCasePath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetCasePath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseTrackerPath(caseUrn, caseId),
                correlationId);
        }

        public async Task<HttpResponseMessage> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId),
                correlationId);
        }

        public async Task<HttpResponseMessage> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, polarisDocumentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, polarisDocumentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetRedactDocumentPath(caseUrn, caseId, polarisDocumentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseSearchQueryPath(caseUrn, caseId, searchTerm),
                correlationId);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null)
        {
            var request = _requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}

