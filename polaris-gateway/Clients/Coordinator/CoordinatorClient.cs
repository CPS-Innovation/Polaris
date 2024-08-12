using System.Net;
using System.Text;
using Common.Configuration;
using Common.Constants;
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

        public async Task<HttpResponseMessage> GetDocumentNotes(string caseUrn, int caseId, string cmsAuthValues, int documentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentNotesPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> AddDocumentNote(string caseUrn, int caseId, string cmsAuthValues, int documentId, AddDocumentNoteDto addDocumentNoteRequestDto, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentNotesPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(addDocumentNoteRequestDto), Encoding.UTF8, ContentType.Json));
        }

        public async Task<HttpResponseMessage> GetPii(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetPiiPath(caseUrn, caseId, polarisDocumentId),
                correlationId);
        }

        public async Task<HttpResponseMessage> RenameDocumentAsync(string caseUrn, int caseId, string cmsAuthValues, int documentId, RenameDocumentRequestDto renameDocumentRequestDto, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetRenameDocumentPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(renameDocumentRequestDto), Encoding.UTF8, ContentType.Json));
        }

        public async Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, ModifyDocumentDto modifyDocumentRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetModifyDocumentPath(caseUrn, caseId.ToString(), polarisDocumentId.Value),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(modifyDocumentRequest), Encoding.UTF8, ContentType.Json));
        }

        public async Task<HttpResponseMessage> GetExhibitProducers(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseExhibitProducersPath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
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

