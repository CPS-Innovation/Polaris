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
using Common.Services.PiiService.Domain;

namespace coordinator.Functions
{
    public class ExtractPii : BaseClient
    {
        private readonly int CharacterLimit;
        private readonly ILogger<ExtractPii> _logger;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IPiiService _piiService;


        public ExtractPii(ILogger<ExtractPii> logger, IPolarisBlobStorageService blobStorageService, IPiiService piiService, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
            CharacterLimit = int.Parse(configuration[ConfigKeys.PiiChunkCharacterLimit]);
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
                var existingResultsStream = await _blobStorageService.GetBlobAsync(piiBlobName);
                if (existingResultsStream != null)
                {

                }
                var ocrResultsTask = _blobStorageService.GetObjectAsync<AnalyzeResults>(
                    BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Ocr));
                var piiResultsTask = _blobStorageService.GetObjectAsync<PiiEntitiesWrapper>(
                    BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Pii));
                await Task.WhenAll(ocrResultsTask, piiResultsTask);
                var ocrResults = await ocrResultsTask;
                var piiResults = await piiResultsTask;

                if (document.PiiVersionId != null &&
                    document.PiiVersionId == document.VersionId &&
                    ocrResults != null && piiResults != null)
                {
                    var piiChunks = _piiChunkingService.GetDocumentTextPiiChunks(ocrResults, caseId, documentId, CharacterLimit, currentCorrelationId);
                    var results = _piiService.ReconcilePiiResults(piiChunks, piiResults);

                    return new OkObjectResult(results);
                }
                else
                {
                    if (ocrResults == null) return new EmptyResult(); // need to handle this

                    var results = await _piiService.GetPiiResults(ocrResults, caseId, documentId, CharacterLimit, currentCorrelationId);


                    var caseEntityKey = RefreshCaseOrchestrator.GetKey(caseId);
                    var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);

                    await client.SignalEntityAsync<ICaseDurableEntity>(
                        caseEntityId,
                        x => x.SetPiiVersionId(documentId));

                    return new OkObjectResult(results);
                }
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(ExtractPii), currentCorrelationId, ex);
            }
        }
    }
}