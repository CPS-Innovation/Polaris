using Common.Configuration;
using Common.Exceptions;
using Common.Dto.Request.Search;
using Common.Extensions;
using text_extractor.Services.CaseSearchService;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace text_extractor.Functions
{
    public class SearchText : BaseFunction
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public SearchText(
            ISearchIndexService searchIndexService,
            IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _searchIndexService = searchIndexService;
            _jsonConvertWrapper = jsonConvertWrapper;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        [Function(nameof(SearchText))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.CaseSearch)] HttpRequest request, string caseUrn, int caseId)
        {
            var correlationId = request.Headers.GetCorrelationId();
            _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

            if (request.Body == null)
            {
                throw new BadRequestException("Request body has no content", nameof(request));
            }
            var content = await request.GetRawBodyStringAsync();
            var searchDto = _jsonConvertWrapper.DeserializeObject<SearchRequestDto>(content);

            var searchResults = await _searchIndexService.QueryAsync(
                caseId,
                searchDto.SearchTerm);

            return CreateJsonResult(searchResults);
        }
    }
}