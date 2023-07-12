using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Domain.Exceptions;
using Common.Dto.Request.Search;
using Common.Extensions;
using Common.Mappers.Contracts;
using Common.Services.CaseSearchService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace text_extractor.Functions
{
    public class SearchText
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly ISearchFilterDocumentMapper _searchFilterDocumentMapper;
        private readonly JsonConvertWrapper _jsonConvertWrapper;

        public SearchText(ISearchIndexService searchIndexService, ISearchFilterDocumentMapper searchFilterDocumentMapper, JsonConvertWrapper jsonConvertWrapper)
        {
            _searchIndexService = searchIndexService;
            _searchFilterDocumentMapper = searchFilterDocumentMapper;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName(nameof(SearchText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "search")] HttpRequestMessage request)
        {
            var correlationId = request.Headers.GetCorrelationId();

            if (request.Content == null)
            {
                throw new BadRequestException("Request body has no content", nameof(request));
            }
            var content = await request.Content.ReadAsStringAsync();
            var searchDto = _jsonConvertWrapper.DeserializeObject<SearchRequestDto>(content);

            var searchFilterDocuments = searchDto.Documents.Select(_searchFilterDocumentMapper.MapToSearchFilterDocument).ToList();

            var searchResults = await _searchIndexService.QueryAsync(
                searchDto.CaseId,
                searchFilterDocuments,
                searchDto.SearchTerm,
                correlationId);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(_jsonConvertWrapper.SerializeObject(searchResults))
            };
        }
    }
}