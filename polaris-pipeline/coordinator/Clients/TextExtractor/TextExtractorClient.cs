using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.SearchIndex;
using Common.Dto.Response;
using Common.Wrappers;
using Common.Handlers;

namespace coordinator.Clients.TextExtractor
{
    public class TextExtractorClient : ITextExtractorClient
    {
        private const string DocumentId = nameof(DocumentId);
        private readonly HttpClient _httpClient;
        private readonly IRequestFactory _requestFactory;
        private readonly ISearchDtoContentFactory _searchDtoContentFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TextExtractorClient(
            HttpClient httpClient,
            IRequestFactory requestFactory,
            ISearchDtoContentFactory searchDtoContentFactory,
            IJsonConvertWrapper jsonConvertWrapper
            )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
            _searchDtoContentFactory = searchDtoContentFactory ?? throw new ArgumentNullException(nameof(searchDtoContentFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        public async Task<StoreCaseIndexesResult> StoreCaseIndexesAsync(string documentId, string urn, long caseId, long versionId, string blobName, Guid correlationId, Stream ocrResults)
        {
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetExtractPath(urn, caseId, documentId, versionId), correlationId);
            request.Headers.Add(DocumentId, documentId);
            // BlobName header is deprecated and will be removed in the future
            request.Headers.Add("BlobName", blobName);

            using var requestContent = new StreamContent(ocrResults);
            request.Content = requestContent;

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var responseContent = await response.Content.ReadAsStringAsync();

            StoreCaseIndexesResult result;

            if (response.IsSuccessStatusCode)
            {
                result = _jsonConvertWrapper.DeserializeObject<StoreCaseIndexesResult>(responseContent);
            }
            else
            {
                var unsuccessfulResponse = _jsonConvertWrapper.DeserializeObject<ExceptionContent>(responseContent);
                result = _jsonConvertWrapper.DeserializeObject<StoreCaseIndexesResult>(unsuccessfulResponse?.Data.ToString());
            }

            return result;
        }

        public async Task<IList<StreamlinedSearchLine>> SearchTextAsync(
            string urn,
            long caseId,
            string searchTerm,
            Guid correlationId
        )
        {
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetSearchPath(urn, caseId), correlationId);
            request.Content = _searchDtoContentFactory.Create(searchTerm);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(responseContent);
            }
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string urn, long caseId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetRemoveCaseIndexesPath(urn, caseId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexDocumentsDeletedResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetCaseIndexCount(string urn, long caseId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Get, RestApi.GetCaseIndexCountResultsPath(urn, caseId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetDocumentIndexCount(string urn, long caseId, string documentId, long versionId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Get, RestApi.GetDocumentIndexCountResultsPath(urn, caseId, documentId, versionId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }
    }
}