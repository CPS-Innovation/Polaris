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

namespace text_extractor.Functions
{
    public class SearchText
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

        [FunctionName(nameof(SearchText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.CaseSearch)] HttpRequestMessage request, string caseUrn, int caseId)
        {
            var correlationId = request.Headers.GetCorrelationId();
            _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

            if (request.Content == null)
            {
                throw new BadRequestException("Request body has no content", nameof(request));
            }
            var content = await request.Content.ReadAsStringAsync();
            var searchDto = _jsonConvertWrapper.DeserializeObject<SearchRequestDto>(content);

            var searchResults = await _searchIndexService.QueryAsync(
                caseId,
                searchDto.SearchTerm);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(_jsonConvertWrapper.SerializeObject(searchResults))
            };
        }
    }
}