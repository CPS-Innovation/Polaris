using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Wrappers;
using Common.Helpers;

namespace coordinator.Clients.PdfRedactor
{
    public class PdfRedactorClient : IPdfRedactorClient
    {
        private readonly IRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public PdfRedactorClient(IRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        public async Task<Stream> RedactPdfAsync(string caseUrn, int caseId, string materialId, long documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId, bool isLegacy = true)
        {
            try
            {
                LegacyCaseValidation.EnsureCaseUrnProvided(caseUrn, isLegacy);
                var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json");

                var path = isLegacy
                    ? RestApi.GetRedactPdfPathLegacy(caseUrn, caseId, materialId, documentId)
                    : RestApi.GetRedactPdfPath(caseId, materialId, documentId);

                var request = _pipelineClientRequestFactory.Create(HttpMethod.Put, path, correlationId);
                request.Content = requestMessage;

                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync();
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // todo: check if ok to swallow a not found response?
                    return null;
                }
                throw;
            }
        }

        public async Task<Stream> ModifyDocument(string caseUrn, int caseId, string materialId, long documentId, ModifyDocumentWithDocumentDto modifyDocumentDto, Guid correlationId, bool isLegacy = true)
        {
            try
            {
                LegacyCaseValidation.EnsureCaseUrnProvided(caseUrn, isLegacy);

                var requestMessage = new StringContent(JsonSerializer.Serialize(modifyDocumentDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), Encoding.UTF8, "application/json");

                var path = isLegacy
                    ? RestApi.GetModifyDocumentPathLegacy(caseUrn, caseId, materialId, documentId)
                    : RestApi.GetModifyDocumentPath(caseId, materialId, documentId);

                var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, path, correlationId);
                request.Content = requestMessage;

                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync();
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // todo: check if ok to swallow a not found response?
                    return null;
                }
                throw;
            }
        }
    }
}
