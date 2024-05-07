using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Streaming;
using coordinator.Domain;

namespace coordinator.Clients.PdfGenerator
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private const string FiletypeKey = "Filetype";
        private readonly IRequestFactory _requestFactory;
        private readonly HttpClient _httpClient;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

        public PdfGeneratorClient(IRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory)
        {
            _requestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
        }

        public async Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
        {
            var request = _requestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}", correlationId);
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            request.Headers.Add(FiletypeKey, fileType.ToString());

            using var requestContent = new StreamContent(documentStream);
            request.Content = requestContent;

            // do not dispose or use `using` on response
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                var result = response.Content.ReadAsStringAsync().Result;
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
    }
}
