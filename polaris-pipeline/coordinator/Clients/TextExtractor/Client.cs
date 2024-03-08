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
using Microsoft.Extensions.Configuration;
using Common.Handlers;

namespace coordinator.Clients.TextExtractor
{
    public class Client : IClient
    {
        private readonly HttpClient _httpClient;
        private readonly IRequestFactory _requestFactory;
        private readonly ISearchDtoContentFactory _searchDtoContentFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public Client(
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
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetExtractPath(cmsCaseUrn, cmsCaseId, cmsDocumentId, versionId), correlationId);
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
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetSearchPath(caseUrn, cmsCaseId), correlationId);
            request.Content = _searchDtoContentFactory.Create(searchTerm, documents);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(responseContent);
            }
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetRemoveCaseIndexesPath(caseUrn, cmsCaseId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexDocumentsDeletedResult>(result);
            }
        }

        public async Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Post, RestApi.GetWaitForCaseEmptyResultsPath(caseUrn, cmsCaseId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IndexSettledResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetCaseIndexCount(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Get, RestApi.GetCaseIndexCountResultsPath(caseUrn, cmsCaseId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }

        public async Task<SearchIndexCountResult> GetDocumentIndexCount(string caseUrn, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId)
        {
            var request = _requestFactory.Create(HttpMethod.Get, RestApi.GetDocumentIndexCountResultsPath(caseUrn, cmsCaseId, cmsDocumentId, versionId), correlationId);

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<SearchIndexCountResult>(result);
            }
        }
    }
}