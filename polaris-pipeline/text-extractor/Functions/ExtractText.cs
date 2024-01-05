using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.Exceptions;
using Common.Dto.Request;
using Common.Extensions;
using Common.Handlers.Contracts;
using Common.Mappers.Contracts;
using Common.Services.CaseSearchService.Contracts;
using Common.Services.OcrService;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers.Contracts;
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
        private readonly ILogger<ExtractText> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public ExtractText(IValidatorWrapper<ExtractTextRequestDto> validatorWrapper,
                           IOcrService ocrService,
                           ISearchIndexService searchIndexService,
                           IExceptionHandler exceptionHandler,
                           IDtoHttpRequestHeadersMapper dtoHttpRequestHeadersMapper,
                           ILogger<ExtractText> logger,
                           ITelemetryClient telemetryClient,
                           ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _validatorWrapper = validatorWrapper ?? throw new ArgumentNullException(nameof(validatorWrapper));
            _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _dtoHttpRequestHeadersMapper = dtoHttpRequestHeadersMapper ?? throw new ArgumentNullException(nameof(dtoHttpRequestHeadersMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
        }

        [FunctionName(nameof(ExtractText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.Extract)] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            IndexedDocumentEvent telemetryEvent = default;
            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);
                telemetryEvent = new IndexedDocumentEvent(currentCorrelationId);

                if (request.Content == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

                // map our request headers to our dto so that we can make use of the validator rules against the dto.
                var extractTextRequest = _dtoHttpRequestHeadersMapper.Map<ExtractTextRequestDto>(request.Headers);
                var results = _validatorWrapper.Validate(extractTextRequest);
                if (results.Any())
                {
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                }

                _telemetryAugmentationWrapper.RegisterDocumentId(extractTextRequest.DocumentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(extractTextRequest.VersionId.ToString());

                telemetryEvent.CaseId = extractTextRequest.CaseId;
                telemetryEvent.DocumentId = extractTextRequest.DocumentId;
                telemetryEvent.VersionId = extractTextRequest.VersionId;
                telemetryEvent.StartTime = DateTime.UtcNow;

                var inputStream = await request.Content.ReadAsStreamAsync();
                var ocrResults = await _ocrService.GetOcrResultsAsync(inputStream, currentCorrelationId);
                telemetryEvent.OcrCompletedTime = DateTime.UtcNow;
                telemetryEvent.PageCount = ocrResults.ReadResults.Count;
                telemetryEvent.LineCount = ocrResults.ReadResults.Sum(x => x.Lines.Count);
                telemetryEvent.WordCount = ocrResults.ReadResults.Sum(x => x.Lines.Sum(y => y.Words.Count));

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

                telemetryEvent.IndexStoredTime = DateTime.UtcNow;

                if (await _searchIndexService.WaitForStoreResultsAsync(ocrResults, extractTextRequest.CaseId, extractTextRequest.DocumentId, extractTextRequest.VersionId, currentCorrelationId))
                {
                    telemetryEvent.EndTime = DateTime.UtcNow;
                    _telemetryClient.TrackEvent(telemetryEvent);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                throw new Exception("Search index update failed, timeout waiting for indexation validation");
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _exceptionHandler.HandleException(exception, currentCorrelationId, nameof(ExtractText), _logger);
            }
        }
    }
}