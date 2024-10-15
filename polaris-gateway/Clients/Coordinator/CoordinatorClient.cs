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

        // public async Task<HttpResponseMessage> GetUrnFromCaseIdAsync(int caseId, string cmsAuthValues, Guid correlationId)
        // {
        //     return await SendRequestAsync(
        //         HttpMethod.Get,
        //         RestApi.GetUrnLookupPath(caseId),
        //         correlationId,
        //         cmsAuthValues);
        // }

        // public async Task<HttpResponseMessage> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId)
        // {
        //     return await SendRequestAsync(
        //         HttpMethod.Get,
        //         RestApi.GetCasesPath(caseUrn),
        //         correlationId,
        //         cmsAuthValues);
        // }

        // public async Task<HttpResponseMessage> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        // {
        //     return await SendRequestAsync(
        //         HttpMethod.Get,
        //         RestApi.GetCasePath(caseUrn, caseId),
        //         correlationId,
        //         cmsAuthValues);
        // }

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

        public async Task<HttpResponseMessage> GetDocumentAsync(string caseUrn, int caseId, string documentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentPath(caseUrn, caseId, documentId),
                correlationId);
        }

        public async Task<HttpResponseMessage> CheckoutDocumentAsync(string caseUrn, int caseId, string documentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> CancelCheckoutDocumentAsync(string caseUrn, int caseId, string documentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues);
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

        public async Task<HttpResponseMessage> GetDocumentNotes(string caseUrn, int caseId, string cmsAuthValues, string documentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentNotesPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> AddDocumentNote(string caseUrn, int caseId, string cmsAuthValues, string documentId, AddDocumentNoteDto addDocumentNoteRequestDto, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentNotesPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(addDocumentNoteRequestDto), Encoding.UTF8, ContentType.Json));
        }

        public async Task<HttpResponseMessage> GetPii(string caseUrn, int caseId, string documentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetPiiPath(caseUrn, caseId, documentId),
                correlationId);
        }

        public async Task<HttpResponseMessage> RenameDocumentAsync(string caseUrn, int caseId, string cmsAuthValues, string documentId, RenameDocumentRequestDto renameDocumentRequestDto, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetRenameDocumentPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(renameDocumentRequestDto), Encoding.UTF8, ContentType.Json));
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

        public async Task<HttpResponseMessage> ReclassifyDocument(string caseUrn, int caseId, string documentId, ReclassifyDocumentDto reclassifyDocumentDto, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetReclassifyDocumentPath(caseUrn, caseId.ToString(), documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(reclassifyDocumentDto), Encoding.UTF8, ContentType.Json));
        }

        public async Task<HttpResponseMessage> GetCaseExhibitProducers(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseExhibitProducersPath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GetCaseWitnesses(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseWitnessesPath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GetMaterialTypeListAsync(string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.MaterialTypeList,
                correlationId,
                cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GetWitnessStatementsAsync(string caseUrn, int caseId, int witnessId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetWitnessStatementsPath(caseUrn, caseId, witnessId),
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

