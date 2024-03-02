using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Net.Http;
using Common.Constants;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using coordinator.Durable.Entity;
using Common.ValueObjects;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Functions
{
    public record GetTrackerDocumentResponse
    {
        internal bool Success;
        internal IActionResult Error;
        internal Guid CorrelationId;
        internal CmsDocumentEntity CmsDocument;
        internal PcdRequestEntity PcdRequest;
        internal DefendantsAndChargesEntity DefendantsAndCharges;

        public string GetBlobName()
        {
            return CmsDocument?.PdfBlobName ?? PcdRequest?.PdfBlobName ?? DefendantsAndCharges.PdfBlobName;
        }
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
                PolarisDocumentId polarisDocumentId,
                ILogger log
            )
        {
            var response = new GetTrackerDocumentResponse { Success = false };

            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                response.Error = new BadRequestObjectResult(correlationErrorMessage);
                return response;
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out response.CorrelationId))
                if (response.CorrelationId == Guid.Empty)
                {
                    response.Error = new BadRequestObjectResult(correlationErrorMessage);
                    return response;
                }

            var entityId = new EntityId(nameof(CaseDurableEntity), RefreshCaseOrchestrator.GetKey(caseId));
            var stateResponse = await client.ReadEntityStateAsync<CaseDurableEntity>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                response.Error = new NotFoundObjectResult(baseMessage);
                return response;
            }

            CaseDurableEntity entityState = stateResponse.EntityState;
            response.CmsDocument = entityState.CmsDocuments.FirstOrDefault(doc => doc.PolarisDocumentId.Equals(polarisDocumentId));
            if (response.CmsDocument == null)
            {
                response.PcdRequest = entityState.PcdRequests.FirstOrDefault(pcd => pcd.PolarisDocumentId.Equals(polarisDocumentId));

                if (response.PcdRequest == null)
                {
                    if (polarisDocumentId.Equals(entityState.DefendantsAndCharges.PolarisDocumentId))
                    {
                        response.DefendantsAndCharges = entityState.DefendantsAndCharges;
                    }
                    else
                    {
                        var baseMessage = $"No Document found with id '{polarisDocumentId}'";
                        response.Error = new NotFoundObjectResult(baseMessage);
                        return response;
                    }
                }
            }

            response.Success = true;

            return response;
        }
    }
}
