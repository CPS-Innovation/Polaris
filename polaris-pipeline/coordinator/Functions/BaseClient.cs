using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using System.Linq;
using coordinator.Durable.Entity;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Extensions.Logging;
using Common.Logging;

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
                IDurableEntityClient client,
                int caseId,
                string documentId,
                ILogger logger,
                Guid currentCorrelationId,
                string loggerSource
            )
        {
            var response = new GetTrackerDocumentResponse { Success = false };
            CaseDurableEntity entityState;

            var entityId = new EntityId(nameof(CaseDurableEntity), RefreshCaseOrchestrator.GetKey(caseId));

            try
            {
                var stateResponse = await client.ReadEntityStateAsync<CaseDurableEntity>(entityId);
                if (!stateResponse.EntityExists)
                {
                    throw new Exception($"No pipeline tracker found with id '{caseId}'");
                }
                entityState = stateResponse.EntityState;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error when retrieving entity for {loggerSource}: {ex.Message}";

                logger.LogMethodError(currentCorrelationId, loggerSource, errorMessage, ex);

                response.Error = new NotFoundObjectResult(errorMessage);
                return response;
            }

            response.CmsDocument = entityState.CmsDocuments.FirstOrDefault(doc => doc.DocumentId.Equals(documentId));
            if (response.CmsDocument == null)
            {
                response.PcdRequest = entityState.PcdRequests.FirstOrDefault(pcd => pcd.DocumentId.Equals(documentId));

                if (response.PcdRequest == null)
                {
                    if (documentId.Equals(entityState.DefendantsAndCharges.DocumentId))
                    {
                        response.DefendantsAndCharges = entityState.DefendantsAndCharges;
                    }
                    else
                    {
                        var baseMessage = $"No Document found with id '{documentId}'";
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
