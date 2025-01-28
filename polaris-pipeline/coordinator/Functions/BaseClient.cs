using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Microsoft.DurableTask.Client;
using coordinator.Domain;
using Common.Services.BlobStorage;
using Common.Configuration;
using Microsoft.Extensions.Configuration;

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
    }

    public class BaseClient
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;

        public BaseClient(
            IConfiguration configuration,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        }

        protected async Task<GetTrackerDocumentResponse> GetTrackerDocument(
            DurableTaskClient client,
            int caseId,
            string documentId,
            ILogger logger,
            Guid currentCorrelationId,
            string loggerSource)
        {
            var response = new GetTrackerDocumentResponse { Success = false };
            CaseDurableEntityState entityState;

            var entityId = CaseDurableEntity.GetEntityId(caseId);

            try
            {
                var stateResponse = await client.Entities.GetEntityAsync<CaseDurableEntityState>(entityId);

                if (stateResponse is null || stateResponse?.IncludesState != true)
                {
                    throw new Exception($"No pipeline tracker found with id '{caseId}'");
                }

                entityState = stateResponse.State;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error when retrieving entity for {loggerSource}: {ex.Message}";

                logger.LogMethodError(currentCorrelationId, loggerSource, errorMessage, ex);

                response.Error = new NotFoundObjectResult(errorMessage);
                return response;
            }

            var blobId = new BlobIdType(caseId, default, default, BlobType.DocumentList);
            var documentsState = (await _polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(blobId)) ?? new CaseDurableEntityDocumentsState();

            response.CmsDocument = documentsState.CmsDocuments.FirstOrDefault(doc => doc.DocumentId.Equals(documentId));
            if (response.CmsDocument == null)
            {
                response.PcdRequest = documentsState.PcdRequests.FirstOrDefault(pcd => pcd.DocumentId.Equals(documentId));

                if (response.PcdRequest == null)
                {
                    if (documentId.Equals(documentsState.DefendantsAndCharges.DocumentId))
                    {
                        response.DefendantsAndCharges = documentsState.DefendantsAndCharges;
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