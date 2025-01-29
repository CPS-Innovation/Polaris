using System.Threading.Tasks;
using Common.Configuration;
using Common.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Durable.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using System;
using Common.Services.BlobStorage;
using coordinator.Domain;
using Microsoft.Extensions.Configuration;

namespace coordinator.Functions
{
    public class GetTracker
    {
        const string loggingName = $"{nameof(GetTracker)} - {nameof(HttpStart)}";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ICaseDurableEntityMapper _caseDurableEntityMapper;
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ILogger<GetTracker> _logger;

        public GetTracker(
            IJsonConvertWrapper jsonConvertWrapper,
            IConfiguration configuration,
            ICaseDurableEntityMapper caseDurableEntityMapper,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            ILogger<GetTracker> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _caseDurableEntityMapper = caseDurableEntityMapper;
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _logger = logger;
        }

        [Function(nameof(GetTracker))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseTracker)] HttpRequest req,
            string caseUrn,
            int caseId,
            [DurableClient] DurableTaskClient client)
        {
            // todo: temporary code
            var entityId = CaseDurableEntity.GetEntityId(caseId);
            EntityMetadata<CaseDurableEntityState> caseEntity;
            try
            {
                caseEntity = await client.Entities.GetEntityAsync<CaseDurableEntityState>(entityId, true);
            }
            catch (Exception ex)
            {
                // #23618 - Race condition: if a case orchestrator has just been kicked off then there is a possibility that 
                //  the entity calls that create (or reset) the entity are still queued up by the time the UI calls
                //  this endpoint. In this scenario, a StorageException is thrown and we are told the blob does not exist.
                //  AppInsights so far shows the orchestrator eventually executes and the entity becomes available, so
                //  lets just let the caller have the same experience as `!caseEntity.EntityExists`.
                // Note: the first implementation for the fix was to catch StorageException (which was what was in the App Insights logs).
                //  Falling back to catch Exception as we are not sure if the StorageException is the only exception that can be thrown.
                return new NotFoundObjectResult($"No Case Entity found with id '{caseId}' with exception '{ex.GetType().Name}: {ex.Message}");
            }

            if (caseEntity is null || caseEntity?.IncludesState != true)
            {
                return new NotFoundObjectResult($"No Case Entity found with id '{caseId}'");
            }

            var blobId = new BlobIdType(caseId, default, default, BlobType.DocumentList);
            var documentsList = (await _polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(blobId)) ?? new CaseDurableEntityDocumentsState();

            var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity.State, documentsList);
            return new OkObjectResult(trackerDto);
        }
    }
}