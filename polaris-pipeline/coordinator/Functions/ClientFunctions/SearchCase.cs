using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients;
using Common.Constants;
using Common.Logging;
using coordinator.Domain.Tracker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ClientFunctions
{
    public class SearchCase
    {
        private readonly ISearchIndexClient _searchIndexClient;

        public SearchCase(ISearchIndexClient searchIndexClient)
        {
            _searchIndexClient = searchIndexClient;
        }

        [FunctionName(nameof(SearchCase))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "urns/{caseUrn}/cases/{caseId}/documents/search/{*searchTerm}")] HttpRequestMessage req,
            string caseUrn,
            int caseId,
            string searchTerm, 
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            const string loggingName = $"{nameof(GetDocument)} - {nameof(HttpStart)}";
            const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

            if (string.IsNullOrWhiteSpace(searchTerm))
                return new BadRequestObjectResult("Search term not supplied.");

            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                return new BadRequestObjectResult(correlationErrorMessage);
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId))
                if (currentCorrelationId == Guid.Empty)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

            log.LogMethodEntry(currentCorrelationId, loggingName, $"Searching Casewith urn {caseUrn} and caseId {caseId} for term '{searchTerm}'");

            var entityId = new EntityId(nameof(Domain.Tracker), caseId.ToString());
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            var searchResults = await _searchIndexClient.Query(caseId, searchTerm, currentCorrelationId);

            return new OkObjectResult(searchResults);
        }
    }
}
