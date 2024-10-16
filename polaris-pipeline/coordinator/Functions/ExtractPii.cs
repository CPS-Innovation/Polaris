using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Services.PiiService.TextAnalytics;
using coordinator.Constants;
using coordinator.Durable.Entity;
using coordinator.Durable.Orchestration;
using coordinator.Helpers;
using Common.Services.PiiService;
using Common.Configuration;
using Common.Extensions;
using Common.Helpers;
using Common.Services.BlobStorageService;
using Newtonsoft.Json;
using Common.Domain.Ocr;
using Common.Services.PiiService.Domain;
using Common.Services.PiiService.Chunking;

namespace coordinator.Functions
{
    public class ExtractPii : BaseClient
    {
        private readonly int CharacterLimit;
        private readonly ILogger<ExtractPii> _logger;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IPiiChunkingService _piiChunkingService;
        private readonly IPiiService _piiService;
        private readonly ITextAnalysisClient _textAnalysisClient;

        public ExtractPii(ILogger<ExtractPii> logger, IPolarisBlobStorageService blobStorageService, IPiiChunkingService piiChunkingService, IPiiService piiService, ITextAnalysisClient textAnalysisClient, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _piiChunkingService = piiChunkingService ?? throw new ArgumentNullException(nameof(piiChunkingService));
            _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
            _textAnalysisClient = textAnalysisClient ?? throw new ArgumentNullException(nameof(textAnalysisClient));
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


                var ocrResultsTask = _blobStorageService.GetJsonBlobAsync<AnalyzeResults>(
                    BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Ocr));
                var piiResultsTask = _blobStorageService.GetJsonBlobAsync<PiiEntitiesWrapper>(
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

                    var piiChunks = _piiChunkingService.GetDocumentTextPiiChunks(ocrResults, caseId, documentId, CharacterLimit, currentCorrelationId);
                    var piiRequests = _piiService.CreatePiiRequests(piiChunks);

                    var calls = piiRequests.Select(async piiRequest => await _textAnalysisClient.CheckForPii(piiRequest));
                    var piiRequestResults = await Task.WhenAll(calls);

                    var piiResultsWrapper = _piiService.MapPiiResults(piiRequestResults);

                    var jsonResults = JsonConvert.SerializeObject(piiResultsWrapper);
                    var piiBlobName = BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Pii);

                    using (var piiStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults)))
                    {
                        await _blobStorageService.UploadBlobAsync(piiStream, piiBlobName);
                    }

                    //Telemetry stats for future use...
                    // var piiEntityCount = piiResultsWrapper.PiiResultCollection.Sum(x => x.Items.Sum(resultCollection => resultCollection.Entities.Count));
                    // var hasError = piiResultsWrapper.PiiResultCollection.Any(x => x.Items.Exists(resultCollection => resultCollection.HasError));

                    var results = _piiService.ReconcilePiiResults(piiChunks, piiResultsWrapper);

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