using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using coordinator.Constants;
using coordinator.Durable.Entity;
using coordinator.Durable.Orchestration;
using coordinator.Helpers;
using Common.Services.PiiService;
using Common.Configuration;
using Common.Extensions;
using Common.Helpers;
using Common.Services.BlobStorageService;
using Common.Domain.Ocr;
using System.Collections.Generic;

namespace coordinator.Functions
{
    public class ExtractPii : BaseClient
    {
        private readonly ILogger<ExtractPii> _logger;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IPiiService _piiService;

        public ExtractPii(ILogger<ExtractPii> logger, IPolarisBlobStorageService blobStorageService, IPiiService piiService, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
        }

        [FunctionName(nameof(ExtractPii))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.PiiResults)] HttpRequest req,
            string caseUrn,
            int caseId,
            string documentId,
            [DurableClient] IDurableEntityClient client
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var response = await GetTrackerDocument(client, caseId, documentId, _logger, currentCorrelationId, nameof(ExtractPii));
                var document = response.CmsDocument;

                var piiBlobName = BlobNameHelper.GetBlobName(caseId, documentId, document.VersionId, BlobNameHelper.BlobType.Pii);
                var existingResults = await _blobStorageService.GetObjectAsync<IEnumerable<Common.Domain.Pii.PiiLine>>(piiBlobName);
                if (existingResults != null)
                {
                    return new OkObjectResult(existingResults);
                }

                var ocrResults = await _blobStorageService.GetObjectAsync<AnalyzeResults>(
                    BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Ocr));
                if (ocrResults == null) return new EmptyResult(); // need to handle this

                var piiResults = await _piiService.GetPiiResultsAsync(ocrResults, caseId, documentId, currentCorrelationId);
                await _blobStorageService.UploadObjectAsync(piiResults, piiBlobName);

                var caseEntityKey = RefreshCaseOrchestrator.GetKey(caseId);
                var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);
                await client.SignalEntityAsync<ICaseDurableEntity>(
                    caseEntityId,
                    x => x.SetPiiVersionId(documentId));

                return new OkObjectResult(piiResults);

            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(ExtractPii), currentCorrelationId, ex);
            }
        }
    }
}