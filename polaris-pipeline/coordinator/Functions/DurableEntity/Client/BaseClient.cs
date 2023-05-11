using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Net.Http;
using Common.Constants;
using Microsoft.Extensions.Logging;
using Common.Logging;
using System.Threading.Tasks;
using System.Linq;
using coordinator.Functions.DurableEntity.Entity;
using Common.Dto.Tracker;
using Mapster;

namespace coordinator.Functions.DurableEntity.Client
{
    public record GetTrackerDocumentResponse
    {
        internal bool Success;
        internal IActionResult Error;
        internal Guid CorrelationId;
        internal TrackerCmsDocumentDto CmsDocument;
    }

    public class BaseClient
    {
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        protected async Task<GetTrackerDocumentResponse> GetTrackerDocument
            (
                HttpRequestMessage req,
                IDurableEntityClient client,
                string loggingName,
                string caseId,
                Guid documentId,
                ILogger log
            )
        {
            var response = new GetTrackerDocumentResponse { Success = false };

            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                response.Error = new BadRequestObjectResult(correlationErrorMessage);
                return response;
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out response.CorrelationId))
                if (response.CorrelationId == Guid.Empty)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    response.Error = new BadRequestObjectResult(correlationErrorMessage);
                    return response;
                }

            log.LogMethodEntry(response.CorrelationId, loggingName, caseId);

            var entityId = new EntityId(nameof(TrackerEntity), caseId);
            var stateResponse = await client.ReadEntityStateAsync<TrackerEntity>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogMethodFlow(response.CorrelationId, loggingName, baseMessage);
                response.Error = new NotFoundObjectResult(baseMessage);
                return response;
            }

            var trackerEntity = stateResponse.EntityState;
            var trackerDto = trackerEntity.Adapt<TrackerDto>();

            response.CmsDocument = trackerDto.Documents.FirstOrDefault(doc => doc.PolarisDocumentId == documentId);
            if(response.CmsDocument == null )
            {
                var baseMessage = $"No document found with id '{documentId}'";
                log.LogMethodFlow(response.CorrelationId, loggingName, baseMessage);
                response.Error = new NotFoundObjectResult(baseMessage);
                return response;
            }

            response.Success = true;

            return response;
        }
    }
}
