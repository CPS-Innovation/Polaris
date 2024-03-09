using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Exceptions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using text_extractor.Mappers.Contracts;
using text_extractor.Services.CaseSearchService;
using text_extractor.Services.OcrService;

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
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private const string loggingName = "ExtractText - Run";

        public ExtractText(IValidatorWrapper<ExtractTextRequestDto> validatorWrapper,
                           IOcrService ocrService,
                           ISearchIndexService searchIndexService,
                           IExceptionHandler exceptionHandler,
                           IDtoHttpRequestHeadersMapper dtoHttpRequestHeadersMapper,
                           ILogger<ExtractText> logger,
                           ITelemetryAugmentationWrapper telemetryAugmentationWrapper,
                           IJsonConvertWrapper jsonConvertWrapper)
        {
            _validatorWrapper = validatorWrapper;
            _ocrService = ocrService;
            _searchIndexService = searchIndexService;
            _exceptionHandler = exceptionHandler;
            _dtoHttpRequestHeadersMapper = dtoHttpRequestHeadersMapper;
            _log = logger;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName(nameof(ExtractText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.Extract)] HttpRequestMessage request,
            string caseUrn, long caseId, string documentId, long versionId)
        {
            Guid currentCorrelationId = default;
            ExtractTextResult extractTextResult = new ExtractTextResult();

            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                if (request.Content == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

                // map our request headers to our dto so that we can make use of the validator rules against the dto.
                var extractTextRequest = _dtoHttpRequestHeadersMapper.Map<ExtractTextRequestDto>(request.Headers);
                var results = _validatorWrapper.Validate(extractTextRequest);
                if (results.Any())
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId.ToString());

                var inputStream = await request.Content.ReadAsStreamAsync();
                var ocrResults = await _ocrService.GetOcrResultsAsync(inputStream, currentCorrelationId);
                var ocrLineCount = ocrResults.ReadResults.Sum(x => x.Lines.Count);

                extractTextResult = new ExtractTextResult()
                {
                    OcrCompletedTime = DateTime.UtcNow,
                    PageCount = ocrResults.ReadResults.Count,
                    LineCount = ocrLineCount,
                    WordCount = ocrResults.ReadResults.Sum(x => x.Lines.Sum(y => y.Words.Count)),
                    IsSuccess = true
                };

                await _searchIndexService.SendStoreResultsAsync
                    (
                        ocrResults,
                        extractTextRequest.PolarisDocumentId,
                        caseId,
                        documentId,
                        versionId,
                        extractTextRequest.BlobName,
                        currentCorrelationId
                    );
                extractTextResult.IndexStoredTime = DateTime.UtcNow;

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_jsonConvertWrapper.SerializeObject(extractTextResult), Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception exception)
            {
                extractTextResult.IsSuccess = false;
                return _exceptionHandler.HandleException(exception, currentCorrelationId, loggingName, _log, extractTextResult);
            }
        }
    }
}