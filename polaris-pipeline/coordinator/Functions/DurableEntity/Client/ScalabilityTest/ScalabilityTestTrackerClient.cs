#if SCALABILITY_TEST
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Wrappers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Functions.DurableEntity.Entity;

namespace coordinator.Functions.DurableEntity.Client.Tracker
{
    public class ScalabilityTestTrackerClient
    {
        const string loggingName = $"{nameof(ScalabilityTestTrackerClient)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public ScalabilityTestTrackerClient(IJsonConvertWrapper jsonConvertWrapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName(nameof(ScalabilityTestTrackerClient))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.ScalabilityTestTracker)] HttpRequestMessage req,
            long caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var scalabilityTestEntity = await GetCaseTrackersForEntity(client, caseId, currentCorrelationId, loggingName, log);

                return new OkObjectResult(scalabilityTestEntity);

            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
        private async Task<ScalabilityTestDurableEntity> GetCaseTrackersForEntity
            (
                IDurableEntityClient client,
                long caseId,
                Guid correlationId,
                string loggingName,
                ILogger log
            )
        {
            var scalabilityTestEntityKey = ScalabilityTestDurableEntity.GetOrchestrationKey(caseId);
            var scalabilityTestEntityId = new EntityId(nameof(ScalabilityTestDurableEntity), scalabilityTestEntityKey);
            var scalabilityTestEntity = await client.ReadEntityStateAsync<ScalabilityTestDurableEntity>(scalabilityTestEntityId);

            if (!scalabilityTestEntity.EntityExists)
                return null;

            return scalabilityTestEntity.EntityState;
        }
    }
}
#endif