using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using coordinator.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using Common.Factories.Contracts;
using coordinator.Factories;
using Common.Dto.Response;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;

namespace coordinator.Clients
{
    public class TextExtractorClient : ITextExtractorClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly IPipelineClientSearchRequestFactory _pipelineClientSearchRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TextExtractorClient(
            HttpClient httpClient,
            IConfiguration configuration,
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            IPipelineClientSearchRequestFactory pipelineClientSearchRequestFactory,
            IJsonConvertWrapper jsonConvertWrapper
            )
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _pipelineClientSearchRequestFactory = pipelineClientSearchRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task ExtractTextAsync(
            PolarisDocumentId polarisDocumentId,
            long cmsCaseId,
            string cmsDocumentId,
            long versionId,
            string blobName,
            Guid correlationId,
            Stream documentStream)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.Extract}?code={_configuration[PipelineSettings.PipelineTextExtractorFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.PolarisDocumentId, polarisDocumentId.ToString());
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

        public async Task<IList<StreamlinedSearchLine>> SearchTextAsync(
            long cmsCaseId,
            string searchTerm,
            Guid correlationId,
            IEnumerable<SearchFilterDocument> documents
            )
        {
            var request = _pipelineClientSearchRequestFactory.Create(cmsCaseId, searchTerm, correlationId, documents);
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(content);
            }
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(long cmsCaseId, Guid correlationId)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.RemoveCaseIndexes}?code={_configuration[PipelineSettings.PipelineTextExtractorFunctionAppKey]}", correlationId);
            var content = new RemoveCaseIndexesRequestDto { CaseId = cmsCaseId };
            request.Content = new StringContent(_jsonConvertWrapper.SerializeObject(content), Encoding.UTF8, "application/json");

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexDocumentsDeletedResult>(result);
            }
        }

        public async Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(long cmsCaseId, Guid correlationId)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.WaitForCaseEmptyResults}?code={_configuration[PipelineSettings.PipelineTextExtractorFunctionAppKey]}", correlationId);
            var content = new WaitForCaseEmptyResultsRequestDto { CaseId = cmsCaseId };
            request.Content = new StringContent(_jsonConvertWrapper.SerializeObject(content), Encoding.UTF8, "application/json");

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexSettledResult>(result);
            }
        }
    }
}