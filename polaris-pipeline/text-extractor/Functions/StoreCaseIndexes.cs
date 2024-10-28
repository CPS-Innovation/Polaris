using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Common.Dto.Response;
using Common.Exceptions;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using text_extractor.Mappers.Contracts;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    public class StoreCaseIndexes : BaseFunction
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;

        private readonly ILogger<StoreCaseIndexes> _log;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private const string LoggingName = "StoreCaseIndexes - Run";

        public StoreCaseIndexes(
               ISearchIndexService searchIndexService,
               IExceptionHandler exceptionHandler,
               ILogger<StoreCaseIndexes> logger,
               ITelemetryAugmentationWrapper telemetryAugmentationWrapper,
               IJsonConvertWrapper jsonConvertWrapper)
        {
            _searchIndexService = searchIndexService;
            _exceptionHandler = exceptionHandler;
            _log = logger;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [Function(nameof(StoreCaseIndexes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Extract)] HttpRequest request,
            string caseUrn, long caseId, string documentId, long versionId)
        {
            Guid currentCorrelationId = default;
            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                if (request.Body == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId.ToString());

                var streamReader = new StreamReader(request.Body);
                var content = await streamReader.ReadToEndAsync();
                var ocrResults = _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(content);

                var storedLinesCount = await _searchIndexService.SendStoreResultsAsync
                    (
                        ocrResults,
                        caseId,
                        documentId,
                        versionId,
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