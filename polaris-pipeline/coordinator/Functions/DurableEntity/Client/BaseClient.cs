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
using Common.ValueObjects;
using Common.Domain.Entity;

namespace coordinator.Functions.DurableEntity.Client
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

        protected async Task<(CaseDurableEntity CaseEntity, CaseRefreshLogsDurableEntity CaseRefreshLogsEntity, string errorMessage)> GetCaseTrackersForEntity
            (
                IDurableEntityClient client,
                string caseId,
                Guid correlationId, 
                string loggingName,
                ILogger log
            )
        {
            var caseEntityKey = CaseDurableEntity.GetOrchestrationKey(caseId);
            var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);
            var caseEntity = await client.ReadEntityStateAsync<CaseDurableEntity>(caseEntityId);

            if (!caseEntity.EntityExists)
            {
                var errorMessage = $"No Case Entity found with id '{caseId}'";
                log.LogMethodFlow(correlationId, loggingName, errorMessage);
                return (null, null, errorMessage);
            }

            var caseRefreshLogsEntityKey = CaseRefreshLogsDurableEntity.GetOrchestrationKey(caseId, caseEntity.EntityState.Version);
            var caseRefreshLogsEntityId = new EntityId(nameof(CaseRefreshLogsDurableEntity), caseRefreshLogsEntityKey);
            var caseRefreshLogsEntity = await client.ReadEntityStateAsync<CaseRefreshLogsDurableEntity>(caseRefreshLogsEntityId);

            if(!caseRefreshLogsEntity.EntityExists)
                return (caseEntity.EntityState, new CaseRefreshLogsDurableEntity(), null);

            return (caseEntity.EntityState, caseRefreshLogsEntity.EntityState, null);
        }

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

            #region Validate Inputs
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
            #endregion

            var entityId = new EntityId(nameof(CaseDurableEntity), CaseDurableEntity.GetOrchestrationKey(caseId));
            var stateResponse = await client.ReadEntityStateAsync<CaseDurableEntity>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogMethodFlow(response.CorrelationId, loggingName, baseMessage);
                response.Error = new NotFoundObjectResult(baseMessage);
                return response;
            }

            CaseDurableEntity entityState = stateResponse.EntityState;
            response.CmsDocument = entityState.CmsDocuments.FirstOrDefault(doc => doc.PolarisDocumentId.Equals(polarisDocumentId));
            if(response.CmsDocument == null )
            {
                response.PcdRequest = entityState.PcdRequests.FirstOrDefault(pcd => pcd.PolarisDocumentId.Equals(polarisDocumentId));

                if (response.PcdRequest == null)
                {
                    if(polarisDocumentId.Equals(entityState.DefendantsAndCharges.PolarisDocumentId))
                    {
                        response.DefendantsAndCharges = entityState.DefendantsAndCharges;
                    }
                    else
                    {
                        var baseMessage = $"No Document found with id '{polarisDocumentId}'";
                        log.LogMethodFlow(response.CorrelationId, loggingName, baseMessage);
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
