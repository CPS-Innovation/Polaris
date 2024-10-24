﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Common.Dto.Response;
using Common.Exceptions;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    public class StoreCaseIndexes
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;

        private readonly ILogger<StoreCaseIndexes> _log;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private const string loggingName = "StoreCaseIndexes - Run";

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

        [FunctionName(nameof(StoreCaseIndexes))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Extract)] HttpRequestMessage request,
            string caseUrn, int caseId, string documentId, long versionId)
        {
            Guid currentCorrelationId = default;
            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                if (request.Content == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId.ToString());

                var inputStream = await request.Content.ReadAsStreamAsync();
                var streamReader = new StreamReader(inputStream);
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

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_jsonConvertWrapper.SerializeObject(result), Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception, currentCorrelationId, loggingName, _log, new StoreCaseIndexesResult { IsSuccess = false });
            }
        }
    }
}