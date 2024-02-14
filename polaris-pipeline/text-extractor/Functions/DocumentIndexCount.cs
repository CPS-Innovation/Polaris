using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Common.Handlers.Contracts;
using Common.Logging;
using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using text_extractor.Services.CaseSearchService.Contracts;

namespace text_extractor.Functions
{
    public class DocumentIndexCount
    {
        private readonly ILogger<DocumentIndexCount> _log;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IExceptionHandler _exceptionHandler;
        private const string loggingName = nameof(DocumentIndexCount);

        public DocumentIndexCount(ILogger<DocumentIndexCount> log, ISearchIndexService searchIndexService, IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper, IExceptionHandler exceptionHandler)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        [FunctionName(nameof(DocumentIndexCount))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.DocumentIndexCount)] HttpRequestMessage request, long caseId, string documentId, long versionId)
        {
            Guid correlationId = Guid.Empty;

            try
            {
                correlationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

                _log.LogMethodFlow(correlationId, loggingName, $"Begin index count for case ID {caseId}");

                var result = await _searchIndexService.GetDocumentIndexCount(caseId, documentId, versionId);

                _log.LogMethodFlow(correlationId, loggingName, $"Index count completed for case ID {caseId}");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_jsonConvertWrapper.SerializeObject(result))
                };
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception, correlationId, loggingName, _log);
            }
            finally
            {
                _log.LogMethodExit(correlationId, loggingName, string.Empty);
            }
        }
    }
}