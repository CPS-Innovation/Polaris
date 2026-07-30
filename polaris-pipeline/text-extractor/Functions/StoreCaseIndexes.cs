using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Common.Dto.Response;
using Common.Exceptions;
using Common.Extensions;
using Common.Handlers;
using Common.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    /// <summary>
    /// Represents a function that stores OCR results for a specific case, material, and document based on the provided case ID, material ID, and document ID.
    /// This function is designed to be triggered by an HTTP POST request and is intended to be accessed via the Housekeeping UI front-end.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="StoreCaseIndexes"/> class.
    /// </remarks>
    /// <param name="searchIndexService">The service used to store OCR results for a specific case, material, and document.</param>
    /// <param name="exceptionHandler">Handler to manage exceptions.</param>
    /// <param name="logger">The logger instance used to log information and errors.</param>
    /// <param name="jsonConvertWrapper">JSON wrapper to handle serialization.</param>
    public class StoreCaseIndexes(
           ISearchIndexService searchIndexService,
           IExceptionHandler exceptionHandler,
           ILogger<StoreCaseIndexes> logger,
           IJsonConvertWrapper jsonConvertWrapper) : BaseFunction
    {
        private readonly ISearchIndexService _searchIndexService = searchIndexService;
        private readonly IExceptionHandler _exceptionHandler = exceptionHandler;

        private readonly ILogger<StoreCaseIndexes> _log = logger;
        private readonly IJsonConvertWrapper _jsonConvertWrapper = jsonConvertWrapper;
        private const string LoggingName = "StoreCaseIndexes - Run";

        [Function(nameof(StoreCaseIndexes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Extract)] HttpRequest request,
            int caseId, string materialId, long documentId)
        {
            var currentCorrelationId = request.Headers.GetCorrelationId();
            try
            {
                if (request.Body == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

                var streamReader = new StreamReader(request.Body);
                var content = await streamReader.ReadToEndAsync();
                var ocrResults = _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(content);

                var storedLinesCount = await _searchIndexService.SendStoreResultsAsync
                    (
                        ocrResults,
                        caseId,
                        materialId,
                        documentId,
                        currentCorrelationId
                    );

                var result = new StoreCaseIndexesResult
                {
                    IsSuccess = true,
                    IndexStoredTime = DateTime.UtcNow,
                    LineCount = storedLinesCount
                };

                return CreateJsonResult(result);
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleExceptionNew(exception, currentCorrelationId, LoggingName, _log, new StoreCaseIndexesResult { IsSuccess = false });
            }
        }
    }
}