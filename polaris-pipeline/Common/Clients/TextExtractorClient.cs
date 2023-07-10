using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Constants;
using Common.Factories.Contracts;
using Common.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace Common.Clients
{
    public class TextExtractorClient : ITextExtractorClient
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;

        public TextExtractorClient(
            HttpClient httpClient,
            IConfiguration configuration,
            IPipelineClientRequestFactory pipelineClientRequestFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
        }

        public async Task ExtractTextAsync(PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId, Stream documentStream)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"extract?code={_configuration[PipelineSettings.PipelineTextExtractorFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.PolarisDocumentId, cmsCaseId.ToString());
            request.Headers.Add(HttpHeaderKeys.CaseId, cmsCaseId.ToString());
            request.Headers.Add(HttpHeaderKeys.DocumentId, cmsDocumentId);
            request.Headers.Add(HttpHeaderKeys.VersionId, versionId.ToString());
            request.Headers.Add(HttpHeaderKeys.BlobName, blobName);

            using (var requestContent = new StreamContent(documentStream))
            {
                request.Content = requestContent;

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public Task<string> SearchTextAsync()
        {
            throw new NotImplementedException();
        }
    }
}