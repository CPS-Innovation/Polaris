using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Streaming;
using Common.Wrappers;
using coordinator.Domain;

namespace coordinator.Clients.PdfGenerator
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private const string FiletypeKey = "Filetype";
        private readonly IRequestFactory _requestFactory;
        private readonly HttpClient _httpClient;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public PdfGeneratorClient(IRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _requestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        public async Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
        {
            var request = _requestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}", correlationId);
            request.Headers.Add(FiletypeKey, fileType.ToString());

            using var requestContent = new StreamContent(documentStream);
            request.Content = requestContent;

            // do not dispose or use `using` on response
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                var result = await response.Content.ReadAsStringAsync();
                Enum.TryParse<PdfConversionStatus>(result, out var enumResult);

                return new ConvertToPdfResponse
                {
                    Status = enumResult,
                };
            }

            response.EnsureSuccessStatusCode();
            var streamResult = await _httpResponseMessageStreamFactory.Create(response);

            return new ConvertToPdfResponse
            {
                PdfStream = streamResult,
                Status = PdfConversionStatus.DocumentConverted
            };
        }

        public async Task<Stream> GenerateThumbnail(string caseUrn, string caseId, string documentId, GenerateThumbnailWithDocumentDto thumbnailRequest, Guid correlationId)
        {
            var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(thumbnailRequest), Encoding.UTF8, "application/json");

            var request = _requestFactory.Create(HttpMethod.Post, $"{RestApi.GetGenerateThumbnailPath(caseUrn, caseId, documentId)}", correlationId);
            request.Content = requestMessage;

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
