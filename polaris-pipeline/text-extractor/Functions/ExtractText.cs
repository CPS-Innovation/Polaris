using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.Requests;
using Common.Exceptions.Contracts;
using Common.Logging;
using Common.Services.SearchIndexService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using text_extractor.Services.OcrService;

namespace text_extractor.Functions
{
    public class ExtractText
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<ExtractTextRequest> _validatorWrapper;
        private readonly IOcrService _ocrService;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<ExtractText> _log;

        public ExtractText(IJsonConvertWrapper jsonConvertWrapper,
             IValidatorWrapper<ExtractTextRequest> validatorWrapper, IOcrService ocrService,
             ISearchIndexService searchIndexService, IExceptionHandler exceptionHandler, ILogger<ExtractText> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _ocrService = ocrService;
            _searchIndexService = searchIndexService;
            _exceptionHandler = exceptionHandler;
            _log = logger;
        }

        [FunctionName("ExtractText")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "extract")] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "ExtractText - Run";

            try
            {
                request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                var correlationId = correlationIdValues.First();
                if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.",
                            correlationId);

                _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                
                var extractTextRequest = _jsonConvertWrapper.DeserializeObject<ExtractTextRequest>(content);
                if (extractTextRequest == null)
                    throw new BadRequestException($"An invalid message was received '{content}'", nameof(request));

                var results = _validatorWrapper.Validate(extractTextRequest);
                if (results.Any())
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));

                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Beginning OCR process for blob {extractTextRequest.BlobName}");
                var ocrResults = await _ocrService.GetOcrResultsAsync(extractTextRequest.BlobName, currentCorrelationId);
                
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"OCR processed finished for {extractTextRequest.BlobName}, beginning search index update");
                await _searchIndexService.StoreResultsAsync
                    (
                        ocrResults,
                        extractTextRequest.PolarisDocumentId,
                        extractTextRequest.CmsCaseId, 
                        extractTextRequest.CmsDocumentId, 
                        extractTextRequest.VersionId, 
                        extractTextRequest.BlobName, 
                        currentCorrelationId
                    );
                
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Search index update completed for blob {extractTextRequest.BlobName}");

                return new HttpResponseMessage(HttpStatusCode.OK);
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