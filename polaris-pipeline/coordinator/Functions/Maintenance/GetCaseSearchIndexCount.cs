using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using coordinator.Clients.TextExtractor;
using Microsoft.Azure.Functions.Worker;

namespace coordinator.Functions.Maintenance
{
    public class GetCaseSearchIndexCount
    {
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ILogger<GetCaseSearchIndexCount> _logger;

        public GetCaseSearchIndexCount(
            ITextExtractorClient textExtractorClient,
            ILogger<GetCaseSearchIndexCount> logger)
        {
            _textExtractorClient = textExtractorClient;
            _logger = logger;
        }

        [Function(nameof(GetCaseSearchIndexCount))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearchCount)] HttpRequest req,
            string caseUrn,
            int caseId)
        {
            var currentCorrelationId = req.Headers.GetCorrelationId();

            var searchIndexCount = await _textExtractorClient.GetCaseIndexCount(caseUrn, caseId, currentCorrelationId);

            return new OkObjectResult(searchIndexCount);
        }
    }
}