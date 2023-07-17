using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Clients
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<PdfGeneratorClient> _logger;

        public PdfGeneratorClient(IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<PdfGeneratorClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
        }

        public async Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
        {
            var pdfStream = new MemoryStream();

            documentStream.Seek(0, SeekOrigin.Begin);
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.ConvertToPdf}?code={_configuration[PipelineSettings.PipelineRedactPdfFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            request.Headers.Add(HttpHeaderKeys.CaseId, caseId);
            request.Headers.Add(HttpHeaderKeys.DocumentId, documentId);
            request.Headers.Add(HttpHeaderKeys.VersionId, versionId);
            request.Headers.Add(HttpHeaderKeys.Filetype, fileType.ToString());

            using (var requestContent = new StreamContent(documentStream))
            {
                request.Content = requestContent;

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await response.Content.CopyToAsync(pdfStream);
                    pdfStream.Seek(0, SeekOrigin.Begin);
                }
            }

            return pdfStream;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            HttpResponseMessage response;
            try
            {
                var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest, correlationId), Encoding.UTF8, "application/json");

                var request = _pipelineClientRequestFactory.Create(HttpMethod.Put, $"{RestApi.RedactPdf}?code={_configuration[PipelineSettings.PipelineRedactPdfFunctionAppKey]}", correlationId);
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
