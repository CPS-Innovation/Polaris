using System.Net;
using System.Text;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
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
            var response = await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseTrackerPath(caseUrn, caseId),
                correlationId, skipRetry: true);


            // #27357 we return 404 if 503 or 502 status code is returned. The client handles 404s and continues to poll
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.BadGateway)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return response;
        }

        public async Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, string documentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetRedactDocumentPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(redactPdfRequest), Encoding.UTF8, ContentType.Json));
        }

        public async Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseSearchQueryPath(caseUrn, caseId, searchTerm),
                correlationId);
        }

        public async Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.CaseSearchCountPath(caseUrn, caseId),
                correlationId);
        }

        public async Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, string documentId, ModifyDocumentDto modifyDocumentRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetModifyDocumentPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(modifyDocumentRequest), Encoding.UTF8, ContentType.Json));
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null, bool skipRetry = false)
        {
            var request = _requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
            if (skipRetry)
            {
                request.Headers.Add("X-Skip-Retry", "true");
            }
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}

