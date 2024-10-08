using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Helpers;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using Common.Configuration;
using Common.Extensions;

namespace coordinator.Functions
{
    public class GetPiiResults
    {
        private readonly ILogger<GetPiiResults> _logger;
        private readonly IOcrResultsService _ocrResultsService;
        private readonly IPiiService _piiService;

        public GetPiiResults(ILogger<GetPiiResults> logger, IOcrResultsService ocrResultsService, IPiiService piiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ocrResultsService = ocrResultsService ?? throw new ArgumentNullException(nameof(ocrResultsService));
            _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
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

                var ocrResults = await _ocrResultsService.GetOcrResultsFromBlob(caseId, polarisDocumentId, currentCorrelationId);
                var piiResults = await _piiService.GetPiiResultsFromBlob(caseId, polarisDocumentId, currentCorrelationId);

                if (ocrResults == null || piiResults == null) return new EmptyResult(); // Need to handle this...

                var piiChunks = _ocrResultsService.GetDocumentTextPiiChunks(ocrResults, caseId, polarisDocumentId, 1000, currentCorrelationId);

                var results = _piiService.ReconcilePiiResults(piiChunks, piiResults);

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetPiiResults), currentCorrelationId, ex);
            }
        }
    }
}