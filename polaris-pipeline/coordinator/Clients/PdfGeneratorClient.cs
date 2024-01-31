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
using Microsoft.Extensions.Configuration;

namespace coordinator.Clients
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public PdfGeneratorClient(IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
        {
            documentStream.Seek(0, SeekOrigin.Begin);

            var pdfStream = new MemoryStream();

            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}?code={_configuration[PipelineSettings.PipelineRedactPdfFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
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
