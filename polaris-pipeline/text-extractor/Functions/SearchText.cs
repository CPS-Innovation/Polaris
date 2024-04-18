using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Exceptions;
using Common.Dto.Request.Search;
using Common.Extensions;
using text_extractor.Services.CaseSearchService;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using text_extractor.Mappers.Contracts;

namespace text_extractor.Functions
{
    public class SearchText
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly ISearchFilterDocumentMapper _searchFilterDocumentMapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public SearchText(
            ISearchIndexService searchIndexService,
            ISearchFilterDocumentMapper searchFilterDocumentMapper,
            IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _searchIndexService = searchIndexService;
            _searchFilterDocumentMapper = searchFilterDocumentMapper;
            _jsonConvertWrapper = jsonConvertWrapper;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        [FunctionName(nameof(SearchText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Search)] HttpRequestMessage request, string caseUrn, long caseId)
        {
            var correlationId = request.Headers.GetCorrelationId();
            _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

            if (request.Content == null)
            {
                throw new BadRequestException("Request body has no content", nameof(request));
            }
            var content = await request.Content.ReadAsStringAsync();
            var searchDto = _jsonConvertWrapper.DeserializeObject<SearchRequestDto>(content);

            var searchFilterDocuments = searchDto.Documents
                .Select(_searchFilterDocumentMapper.MapToSearchFilterDocument)
                .ToList();

            var searchResults = await _searchIndexService.QueryAsync(
                caseId,
                searchFilterDocuments,
                searchDto.SearchTerm);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(_jsonConvertWrapper.SerializeObject(searchResults))
            };
        }
    }
}