using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using Common.Wrappers;
using coordinator.Clients.TextAnalytics;
using coordinator.Helpers;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace coordinator.Functions
{
    public class GetPiiResults
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrResultsService _ocrResultsService;
        private readonly IPiiService _piiService;
        private readonly ITextAnalysisClient _textAnalyticsClient;
        private readonly ITelemetryClient _telemetryClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public GetPiiResults(IPolarisBlobStorageService blobStorageService, IOcrResultsService ocrResultsService, IPiiService piiService, ITextAnalysisClient textAnalyticsClient, ITelemetryClient telemetryClient, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _ocrResultsService = ocrResultsService ?? throw new ArgumentNullException(nameof(ocrResultsService));
            _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
            _textAnalyticsClient = textAnalyticsClient ?? throw new ArgumentNullException(nameof(textAnalyticsClient));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        [FunctionName(nameof(GetPiiResults))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.PiiResults)] HttpRequest req,
            string caseUrn,
            int caseId,
            string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var ocrBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Ocr);

                using var jsonStream = await _blobStorageService.GetDocumentAsync(ocrBlobName, currentCorrelationId);

                // Need to handle if OCR results are null;
                var streamReader = new StreamReader(jsonStream);
                var ocrResults = _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(streamReader.ReadToEnd());

                var piiChunks = _ocrResultsService.GetDocumentText(ocrResults, caseId, polarisDocumentId, 1000);

                var piiRequests = _piiService.CreatePiiRequests(piiChunks);

                var calls = piiRequests.Select(async piiRequest => await _textAnalyticsClient.CheckForPii(piiRequest));
                var piiResults = await Task.WhenAll(calls);

                var jsonResults = JsonConvert.SerializeObject(piiResults);
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

                //Telemetry stats
                var piiEntityCount = piiResults.Sum(x => x.Sum(resultCollection => resultCollection.Entities.Count));
                //var piiErrorCount = piiResults.Sum(x => x.Sum(resultCollection => resultCollection.Any));

                return new OkResult();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}