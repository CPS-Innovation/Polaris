using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Entity;
using Common.Logging;
using Common.Services.CaseSearchService.Contracts;
using coordinator.Functions.DurableEntity.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.DurableEntity.Client.Case
{
    public class SearchCaseClient
    {
        private readonly ICaseSearchClient _searchIndexClient;

        public SearchCaseClient(ICaseSearchClient searchIndexClient)
        {
            _searchIndexClient = searchIndexClient;
        }

        const string loggingName = $"{nameof(SearchCaseClient)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        [FunctionName(nameof(SearchCaseClient))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseSearch)] HttpRequestMessage req,
            string caseUrn,
            int caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var searchTerm = req.RequestUri.ParseQueryString()["query"];
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new BadRequestObjectResult("Search term not supplied.");

                req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

                var correlationId = correlationIdValues.FirstOrDefault();
                if (!Guid.TryParse(correlationId, out currentCorrelationId))
                    if (currentCorrelationId == Guid.Empty)
                    {
                        log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                        return new BadRequestObjectResult(correlationErrorMessage);
                    }

                var entityId = new EntityId(nameof(CaseDurableEntity), CaseDurableEntity.GetOrchestrationKey(caseId.ToString()));
                var trackerState = await client.ReadEntityStateAsync<CaseDurableEntity>(entityId);

                if (!trackerState.EntityExists)
                {
                    var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                    log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                    return new NotFoundObjectResult(baseMessage);
                }

                log.LogMethodEntry(currentCorrelationId, loggingName, $"Searching Case with urn {caseUrn} and caseId {caseId} for term '{searchTerm}'");

                CaseDurableEntity entityState = trackerState.EntityState;
                var documents =
                    entityState.CmsDocuments.OfType<BaseDocumentEntity>()
                        .Concat(entityState.PcdRequests)
                        .Append(entityState.DefendantsAndCharges)
                        .ToList();
                var searchResults = await _searchIndexClient.QueryAsync(caseId, documents, searchTerm, currentCorrelationId);

                return new OkObjectResult(searchResults);
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }

        }
    }
}
