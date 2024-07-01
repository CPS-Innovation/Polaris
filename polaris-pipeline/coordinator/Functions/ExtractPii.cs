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
using coordinator.Clients.TextAnalytics;
using coordinator.Constants;
using coordinator.Durable.Entity;
using coordinator.Durable.Orchestration;
using coordinator.Helpers;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using Common.Configuration;
using Common.Dto.Tracker;
using Common.Extensions;
using Common.Services.BlobStorageService;
using Common.ValueObjects;
using Newtonsoft.Json;

namespace coordinator.Functions
{
    public class ExtractPii : BaseClient
    {
        private readonly int CharacterLimit;
        private readonly ILogger<ExtractPii> _logger;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrResultsService _ocrResultsService;
        private readonly IPiiService _piiService;
        private readonly ITextAnalysisClient _textAnalysisClient;

        public ExtractPii(ILogger<ExtractPii> logger, IPolarisBlobStorageService blobStorageService, IOcrResultsService ocrResultsService, IPiiService piiService, ITextAnalysisClient textAnalysisClient, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _ocrResultsService = ocrResultsService ?? throw new ArgumentNullException(nameof(ocrResultsService));
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
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var response = await GetTrackerDocument(client, caseId.ToString(), new PolarisDocumentId(polarisDocumentId), _logger, currentCorrelationId, nameof(ExtractPii));
                var document = response.CmsDocument;

                var ocrResults = await _ocrResultsService.GetOcrResultsFromBlob(caseId, polarisDocumentId, currentCorrelationId);
                var piiResults = await _piiService.GetPiiResultsFromBlob(caseId, polarisDocumentId, currentCorrelationId);

                if (document.PiiCmsVersionId != null &&
                    document.PiiCmsVersionId == document.CmsVersionId &&
                    ocrResults != null && piiResults != null)
                {
                    var piiChunks = _ocrResultsService.GetDocumentTextPiiChunks(ocrResults, caseId, polarisDocumentId, CharacterLimit, currentCorrelationId);
                    var results = _piiService.ReconcilePiiResults(piiChunks, piiResults);

                    return new OkObjectResult(results);
                }
                else
                {
                    if (ocrResults == null) return new EmptyResult(); // need to handle this

                    var piiChunks = _ocrResultsService.GetDocumentTextPiiChunks(ocrResults, caseId, polarisDocumentId, CharacterLimit, currentCorrelationId);
                    var piiRequests = _piiService.CreatePiiRequests(piiChunks);

                    var calls = piiRequests.Select(async piiRequest => await _textAnalysisClient.CheckForPii(piiRequest));
                    var piiRequestResults = await Task.WhenAll(calls);

                    var piiResultsWrapper = _piiService.MapPiiResults(piiRequestResults);

                    var jsonResults = JsonConvert.SerializeObject(piiResultsWrapper);
                    var piiBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Pii);

                    using (var piiStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults)))
                    {
                        await _blobStorageService.UploadDocumentAsync(
                            piiStream,
                            piiBlobName,
                            caseId.ToString(),
                            polarisDocumentId,
                            versionId: "1",
                            currentCorrelationId
                        );
                    }

                    //Telemetry stats for future use...
                    // var piiEntityCount = piiResultsWrapper.PiiResultCollection.Sum(x => x.Items.Sum(resultCollection => resultCollection.Entities.Count));
                    // var hasError = piiResultsWrapper.PiiResultCollection.Any(x => x.Items.Exists(resultCollection => resultCollection.HasError));

                    var results = _piiService.ReconcilePiiResults(piiChunks, piiResultsWrapper);

                    var caseEntityKey = RefreshCaseOrchestrator.GetKey(caseId.ToString());
                    var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);

                    await client.SignalEntityAsync<ICaseDurableEntity>(
                        caseEntityId,
                        x => x.SetPiiCmsVersionId(polarisDocumentId));

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