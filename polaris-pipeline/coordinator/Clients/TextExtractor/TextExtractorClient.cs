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
using Common.Helpers;

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

        public async Task<StoreCaseIndexesResult> StoreCaseIndexesAsync(string materialId, string urn, int caseId, long documentId, Guid correlationId, Stream ocrResults, bool isLegacy = true)
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(urn, isLegacy);
            var path = isLegacy
                ? RestApi.GetExtractPathLegacy(urn, caseId, materialId, documentId)
                : RestApi.GetExtractPath(caseId, materialId, documentId);

            var request = _requestFactory.Create(HttpMethod.Post, path, correlationId);
            request.Headers.Add(DocumentId, materialId);

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
            int caseId,
            string searchTerm,
            Guid correlationId,
            bool isLegacy = true
        )
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(urn, isLegacy);
            var path = isLegacy
                ? RestApi.GetSearchPathLegacy(urn, caseId)
                : RestApi.GetSearchPath(caseId);

            var request = _requestFactory.Create(HttpMethod.Post, path, correlationId);
            request.Content = _searchDtoContentFactory.Create(searchTerm);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(responseContent);
            }
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string urn, int caseId, Guid correlationId, bool isLegacy = true)
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(urn, isLegacy);
            var path = isLegacy
                ? RestApi.GetRemoveCaseIndexesPathLegacy(urn, caseId)
                : RestApi.GetRemoveCaseIndexesPath(caseId);

            var request = _requestFactory.Create(HttpMethod.Post, path, correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexDocumentsDeletedResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetCaseIndexCount(string urn, int caseId, Guid correlationId, bool isLegacy = true)
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(urn, isLegacy);
            var path = isLegacy
                ? RestApi.GetCaseIndexCountResultsPathLegacy(urn, caseId)
                : RestApi.GetCaseIndexCountResultsPath(caseId);

            var request = _requestFactory.Create(HttpMethod.Get, path, correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetDocumentIndexCount(string urn, int caseId, string materialId, long documentId, Guid correlationId, bool isLegacy = true)
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(urn, isLegacy);
            var path = isLegacy
                ? RestApi.GetDocumentIndexCountResultsPathLegacy(urn, caseId, materialId, documentId)
                : RestApi.GetDocumentIndexCountResultsPath(caseId, materialId, documentId);

            var request = _requestFactory.Create(HttpMethod.Get, path, correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }
    }
}
