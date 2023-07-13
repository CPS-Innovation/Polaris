using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Domain.Exceptions;
using Common.Dto.Request;
using Common.Extensions;
using Common.Handlers.Contracts;
using Common.Logging;
using Common.Mappers.Contracts;
using Common.Services.CaseSearchService.Contracts;
using Common.Services.OcrService;
using Common.Telemetry.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using text_extractor.TelemetryEvents;
namespace text_extractor.Functions
{
    public class ExtractText
    {
        private readonly IValidatorWrapper<ExtractTextRequestDto> _validatorWrapper;
        private readonly IOcrService _ocrService;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDtoHttpRequestHeadersMapper _dtoHttpRequestHeadersMapper;
        private readonly ILogger<ExtractText> _log;
        private readonly ITelemetryClient _telemetryClient;

        public ExtractText(IValidatorWrapper<ExtractTextRequestDto> validatorWrapper,
                           IOcrService ocrService,
                           ISearchIndexService searchIndexService,
                           IExceptionHandler exceptionHandler,
                           IDtoHttpRequestHeadersMapper dtoHttpRequestHeadersMapper,
                           ILogger<ExtractText> logger,
                           ITelemetryClient telemetryClient)
        {
            _validatorWrapper = validatorWrapper;
            _ocrService = ocrService;
            _searchIndexService = searchIndexService;
            _exceptionHandler = exceptionHandler;
            _dtoHttpRequestHeadersMapper = dtoHttpRequestHeadersMapper;
            _log = logger;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(ExtractText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "extract")] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "ExtractText - Run";

            try
            {
                #region Validate-Inputs
                currentCorrelationId = request.Headers.GetCorrelationId();

                if (request.Content == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

                // map our request headers to our dto so that we can make use of the validator rules against the dto.
                var extractTextRequest = _dtoHttpRequestHeadersMapper.Map<ExtractTextRequestDto>(request.Headers);
                var results = _validatorWrapper.Validate(extractTextRequest);
                if (results.Any())
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                #endregion

                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Beginning OCR process for blob {extractTextRequest.BlobName}");

                var startTime = DateTime.UtcNow;

                var inputStream = await request.Content.ReadAsStreamAsync();
                var ocrResults = await _ocrService.GetOcrResultsAsync(inputStream, currentCorrelationId);

                var ocrCompletedTime = DateTime.UtcNow;
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"OCR processed finished for {extractTextRequest.BlobName}, beginning search index update");

                await _searchIndexService.SendStoreResultsAsync
                    (
                        ocrResults,
                        extractTextRequest.PolarisDocumentId,
                        extractTextRequest.CaseId,
                        extractTextRequest.DocumentId,
                        extractTextRequest.VersionId,
                        extractTextRequest.BlobName,
                        currentCorrelationId
                    );
                var indexStoredTime = DateTime.UtcNow;
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Search index update completed for blob {extractTextRequest.BlobName}");

                if (await _searchIndexService.WaitForStoreResultsAsync(ocrResults, extractTextRequest.CaseId, extractTextRequest.DocumentId, extractTextRequest.VersionId, currentCorrelationId))
                {
                    _telemetryClient.TrackEvent(new IndexedDocumentEvent(
                        correlationId: currentCorrelationId,
                        caseId: extractTextRequest.CaseId,
                        documentId: extractTextRequest.DocumentId,
                        versionId: extractTextRequest.VersionId,
                        pageCount: ocrResults.ReadResults.Count,
                        lineCount: ocrResults.ReadResults.Sum(x => x.Lines.Count),
                        wordCount: ocrResults.ReadResults.Sum(x => x.Lines.Sum(y => y.Words.Count)),
                        startTime: startTime,
                        ocrCompletedTime: ocrCompletedTime,
                        indexStoredTime: indexStoredTime,
                        endTime: DateTime.UtcNow
                    ));
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                throw new Exception("Search index update failed, timeout waiting for indexation validation");
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception, currentCorrelationId, loggingName, _log);
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            }
        }
    }
}