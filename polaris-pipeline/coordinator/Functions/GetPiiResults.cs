using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using coordinator.Domain;
using coordinator.Helpers;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using Common.Configuration;
using Common.Extensions;
using Common.Services.BlobStorageService;
using Common.Wrappers;

namespace coordinator.Functions
{
    public class GetPiiResults
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrResultsService _ocrResultsService;
        private readonly IPiiService _piiService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public GetPiiResults(IPolarisBlobStorageService blobStorageService, IOcrResultsService ocrResultsService, IPiiService piiService, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobStorageService = blobStorageService ?? throw new System.ArgumentNullException(nameof(blobStorageService));
            _ocrResultsService = ocrResultsService ?? throw new System.ArgumentNullException(nameof(ocrResultsService));
            _piiService = piiService ?? throw new System.ArgumentNullException(nameof(piiService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new System.ArgumentNullException(nameof(jsonConvertWrapper));
        }

        [FunctionName(nameof(GetPiiResults))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.PiiResults)] HttpRequest req,
            string caseUrn,
            int caseId,
            string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var ocrBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Ocr);
                using var ocrStream = await _blobStorageService.GetDocumentAsync(ocrBlobName, currentCorrelationId);

                // Need to handle if OCR results are null;
                var ocrStreamReader = new StreamReader(ocrStream);
                var ocrResults = _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(ocrStreamReader.ReadToEnd());

                var piiChunks = _ocrResultsService.GetDocumentTextPiiChunks(ocrResults, caseId, polarisDocumentId, 1000);

                var piiBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Pii);
                using var piiStream = await _blobStorageService.GetDocumentAsync(piiBlobName, currentCorrelationId);

                var piiStreamReader = new StreamReader(piiStream);
                var piiResults = _jsonConvertWrapper.DeserializeObject<PiiEntitiesWrapper>(piiStreamReader.ReadToEnd());

                var results = _piiService.ReconcilePiiResults(piiChunks, piiResults);

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}