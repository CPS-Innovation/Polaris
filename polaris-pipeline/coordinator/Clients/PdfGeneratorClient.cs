using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;
using Common.Streaming;
using Microsoft.Extensions.Configuration;

namespace coordinator.Clients
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

        public PdfGeneratorClient(IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
        }

        public async Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
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
                throw new UnsupportedMediaTypeException(
                    $"Unsupported media type: {fileType}",
                    // todo: we do not have the *real* media type header to hand, so just use a generic one
                    //  Key thing is we communicate to the caller that the pdf-generator has rejected us on media type grounds
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                );
            }

            response.EnsureSuccessStatusCode();
            return await _httpResponseMessageStreamFactory.Create(response);
        }
    }
}
