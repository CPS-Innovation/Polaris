using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    public class CaseIndexCount
    {
        private readonly ILogger<CaseIndexCount> _log;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IExceptionHandler _exceptionHandler;
        private const string loggingName = nameof(CaseIndexCount);

        public CaseIndexCount(ILogger<CaseIndexCount> log, ISearchIndexService searchIndexService, IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper, IExceptionHandler exceptionHandler)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        [FunctionName(nameof(CaseIndexCount))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseIndexCount)] HttpRequestMessage request, long caseId)
        {
            Guid correlationId = Guid.Empty;

            try
            {
                correlationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

                var result = await _searchIndexService.GetCaseIndexCount(caseId);

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
        }
    }
}