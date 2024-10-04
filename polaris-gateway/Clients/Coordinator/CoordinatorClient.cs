using System.Net;
using System.Text;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Extensions;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;
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
        
        public async Task<HttpResponseMessage> GetUrnFromCaseIdAsync(int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetUrnLookupPath(caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCasesPath(caseUrn),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCasePath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetCasePath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetCasePath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<IActionResult> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            var response = await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseTrackerPath(caseUrn, caseId),
                correlationId, skipRetry: true);

            // #27357 we return 404 if 503 or 502 status code is returned. The client handles 404s and continues to poll
            var statusCode = response.StatusCode.GetValueOrDefault((int)HttpStatusCode.BadRequest);
            if (statusCode is (int)HttpStatusCode.ServiceUnavailable or (int)HttpStatusCode.BadGateway)
            {
                return new NotFoundResult();
            }

            return response;
        }

        public async Task<FileStreamResult> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            return await SendDocumentRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId),
                correlationId);
        }

        public async Task<ContentResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, polarisDocumentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                RestApi.GetDocumentCheckoutPath(caseUrn, caseId, polarisDocumentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetRedactDocumentPath(caseUrn, caseId, polarisDocumentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(redactPdfRequest), Encoding.UTF8, ContentType.Json));
        }

        public async Task<ContentResult> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseSearchQueryPath(caseUrn, caseId, searchTerm),
                correlationId);
        }

        public async Task<ContentResult> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.CaseSearchCountPath(caseUrn, caseId),
                correlationId);
        }

        public async Task<ContentResult> GetDocumentNotes(string caseUrn, int caseId, string cmsAuthValues, int documentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetDocumentNotesPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> AddDocumentNote(string caseUrn, int caseId, string cmsAuthValues, int documentId, AddDocumentNoteDto addDocumentNoteRequestDto, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetDocumentNotesPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(addDocumentNoteRequestDto), Encoding.UTF8, ContentType.Json));
        }

        public async Task<ContentResult> GetPii(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetPiiPath(caseUrn, caseId, polarisDocumentId),
                correlationId);
        }

        public async Task<ContentResult> RenameDocumentAsync(string caseUrn, int caseId, string cmsAuthValues, int documentId, RenameDocumentRequestDto renameDocumentRequestDto, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Put,
                RestApi.GetRenameDocumentPath(caseUrn, caseId, documentId),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(renameDocumentRequestDto), Encoding.UTF8, ContentType.Json));
        }

        public async Task<ContentResult> ModifyDocument(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, ModifyDocumentDto modifyDocumentRequest, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetModifyDocumentPath(caseUrn, caseId.ToString(), polarisDocumentId.Value),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(modifyDocumentRequest), Encoding.UTF8, ContentType.Json));
        }
        
        public async Task<HttpResponseMessage> ReclassifyDocument(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, ReclassifyDocumentDto reclassifyDocumentDto, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Post,
                RestApi.GetReclassifyDocumentPath(caseUrn, caseId.ToString(), polarisDocumentId.Value),
                correlationId,
                cmsAuthValues,
                new StringContent(JsonConvert.SerializeObject(reclassifyDocumentDto), Encoding.UTF8, ContentType.Json));
        }
        
        public async Task<ContentResult> GetCaseExhibitProducers(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseExhibitProducersPath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> GetCaseWitnesses(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(
                HttpMethod.Get,
                RestApi.GetCaseWitnessesPath(caseUrn, caseId),
                correlationId,
                cmsAuthValues);
        }

        public async Task<ContentResult> GetMaterialTypeListAsync(string cmsAuthValues, Guid correlationId)
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

        private async Task<ContentResult> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null, bool skipRetry = false)
        {
            var request = _requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
            if (skipRetry)
            {
                request.Headers.Add("X-Skip-Retry", "true");
            }
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return await response.ToContentResultAsync();
        }
        
        private async Task<FileStreamResult> SendDocumentRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null, bool skipRetry = false)
        {
            var request = _requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
            if (skipRetry)
            {
                request.Headers.Add("X-Skip-Retry", "true");
            }
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return await response.ToFileStreamResultAsync();
        }
    }
}

