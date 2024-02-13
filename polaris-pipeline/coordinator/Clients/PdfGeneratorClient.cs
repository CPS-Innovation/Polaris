using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using coordinator.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;
using Common.Streaming;
using Microsoft.Extensions.Configuration;

namespace coordinator.Clients
{
    public class ConvertToPdfResponse : IDisposable

    {
        public bool IsSuccess { get; set; }

        public Stream PdfStream { get; set; }

        public string ErrorMessage { get; set; }

        public void Dispose()
        {
            PdfStream?.Dispose();
        }
    }

    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

        public PdfGeneratorClient(IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        public async Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}?code={_configuration[Constants.ConfigKeys.PipelineRedactPdfFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            request.Headers.Add(HttpHeaderKeys.Filetype, fileType.ToString());

            using var requestContent = new StreamContent(documentStream);
            request.Content = requestContent;

            // do not dispose or use `using` on response
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                return new ConvertToPdfResponse
                {
                    IsSuccess = false,
                    ErrorMessage = await response.Content.ReadAsStringAsync()
                };
            }
            response.EnsureSuccessStatusCode();
            return new ConvertToPdfResponse
            {
                IsSuccess = true,
                PdfStream = await _httpResponseMessageStreamFactory.Create(response)
            };
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            HttpResponseMessage response;
            try
            {
                var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest, correlationId), Encoding.UTF8, "application/json");

                var request = _pipelineClientRequestFactory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(caseUrn, caseId, documentId)}?code={_configuration[Constants.ConfigKeys.PipelineRedactPdfFunctionAppKey]}", correlationId);
                request.Content = requestMessage;

                response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();
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

            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent, correlationId);
        }
    }
}
