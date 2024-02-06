using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.Exceptions;
using Common.Dto.Request;
using Common.Extensions;
using Common.Handlers.Contracts;
using Common.Logging;
using text_extractor.Services.CaseSearchService.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace text_extractor.Functions
{
    public class RemoveCaseIndexes
    {
        private readonly ILogger<RemoveCaseIndexes> _log;
        private readonly ISearchIndexService _searchIndexService;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private const string loggingName = nameof(RemoveCaseIndexes);

        public RemoveCaseIndexes(ILogger<RemoveCaseIndexes> logger, ISearchIndexService searchIndexService, ITelemetryAugmentationWrapper telemetryAugmentationWrapper, IExceptionHandler exceptionHandler, IJsonConvertWrapper jsonConvertWrapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        [FunctionName(nameof(RemoveCaseIndexes))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.RemoveCaseIndexes)] HttpRequestMessage request, long caseId)
        {
            Guid correlationId = default;

            try
            {
                correlationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);


                _log.LogMethodFlow(correlationId, loggingName, $"Begin removing case indexes for case ID {caseId}");

                var result = await _searchIndexService.RemoveCaseIndexEntriesAsync(caseId);

                _log.LogMethodFlow(correlationId, loggingName, $"Case indexes removed for case ID {caseId}");

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