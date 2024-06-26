using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Helpers;
using Microsoft.AspNetCore.Http;
using coordinator.Clients.TextExtractor;

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

        [FunctionName(nameof(GetCaseSearchIndexCount))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearchCount)] HttpRequest req,
            string caseUrn,
            int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var searchIndexCount = await _textExtractorClient.GetCaseIndexCount(caseUrn, caseId, currentCorrelationId);

                return new OkObjectResult(searchIndexCount);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(SearchCase), currentCorrelationId, ex);
            }

        }
    }
}
