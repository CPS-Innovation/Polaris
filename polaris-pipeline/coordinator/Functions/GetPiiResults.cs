using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using Common.Wrappers;
using coordinator.Services.OcrResultsService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace coordinator.Functions
{
    public class GetPiiResults
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrResultsService _ocrResultsService;
        private readonly ITelemetryClient _telemetryClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public GetPiiResults(IPolarisBlobStorageService blobStorageService, IOcrResultsService ocrResultsService, ITelemetryClient telemetryClient, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _ocrResultsService = ocrResultsService ?? throw new ArgumentNullException(nameof(ocrResultsService));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        [FunctionName(nameof(GetPiiResults))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", RestApi.PiiResults)] HttpRequest req,
            string caseUrn,
            int caseId,
            string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                using var jsonStream = await _blobStorageService.GetDocumentAsync($"documents/{caseId}/ocrs/{polarisDocumentId}.json", currentCorrelationId);
                var streamReader = new StreamReader(jsonStream);
                var ocrResults = _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(streamReader.ReadToEnd());

                var stuff = _ocrResultsService.GetDocumentText(ocrResults, 1000);


                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}