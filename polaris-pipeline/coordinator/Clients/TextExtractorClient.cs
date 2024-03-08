using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.SearchIndex;
using Common.Constants;
using Common.Dto.Response;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using coordinator.Factories;
using Microsoft.Extensions.Configuration;
using Common.Handlers;

namespace coordinator.Clients
{
    public class TextExtractorClient : ITextExtractorClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly ITextExtractorClientRequestFactory _pipelineClientSearchRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TextExtractorClient(
            HttpClient httpClient,
            IConfiguration configuration,
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            ITextExtractorClientRequestFactory pipelineClientSearchRequestFactory,
            IJsonConvertWrapper jsonConvertWrapper
            )
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _pipelineClientSearchRequestFactory = pipelineClientSearchRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<ExtractTextResult> ExtractTextAsync(
            PolarisDocumentId polarisDocumentId,
            string cmsCaseUrn,
            long cmsCaseId,
            string cmsDocumentId,
            long versionId,
            string blobName,
            Guid correlationId,
            Stream documentStream)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.GetExtractPath(cmsCaseUrn, cmsCaseId, cmsDocumentId, versionId)}?code={_configuration[Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.PolarisDocumentId, polarisDocumentId.ToString());
            request.Headers.Add(HttpHeaderKeys.BlobName, blobName);

            using var requestContent = new StreamContent(documentStream);
            request.Content = requestContent;

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var responseContent = await response.Content.ReadAsStringAsync();

            ExtractTextResult result;

            if (response.IsSuccessStatusCode)
            {
                result = _jsonConvertWrapper.DeserializeObject<ExtractTextResult>(responseContent);
            }
            else
            {
                var unsuccessfulResponse = _jsonConvertWrapper.DeserializeObject<ExceptionContent>(responseContent);
                result = _jsonConvertWrapper.DeserializeObject<ExtractTextResult>(unsuccessfulResponse?.Data.ToString());
            }

            return result;
        }

        public async Task<IList<StreamlinedSearchLine>> SearchTextAsync(
            string caseUrn,
            long cmsCaseId,
            string searchTerm,
            Guid correlationId,
            IEnumerable<SearchFilterDocument> documents
            )
        {
            var request = _pipelineClientSearchRequestFactory.Create(caseUrn, cmsCaseId, searchTerm, correlationId, documents);
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(content);
            }
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.GetRemoveCaseIndexesPath(caseUrn, cmsCaseId)}?code={_configuration[Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey]}", correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexDocumentsDeletedResult>(result);
            }
        }

        public async Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.GetWaitForCaseEmptyResultsPath(caseUrn, cmsCaseId)}?code={_configuration[Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey]}", correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexSettledResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetCaseIndexCount(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Get, $"{RestApi.GetCaseIndexCountResultsPath(caseUrn, cmsCaseId)}?code={_configuration[Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey]}", correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetDocumentIndexCount(string caseUrn, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Get, $"{RestApi.GetDocumentIndexCountResultsPath(caseUrn, cmsCaseId, cmsDocumentId, versionId)}?code={_configuration[Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey]}", correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }
    }
}