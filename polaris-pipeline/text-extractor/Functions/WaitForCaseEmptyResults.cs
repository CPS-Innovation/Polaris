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
using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace text_extractor.Functions
{
    public class WaitForCaseEmptyResults
    {
        private readonly ILogger<WaitForCaseEmptyResults> _log;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly IExceptionHandler _exceptionHandler;
        private const string loggingName = nameof(WaitForCaseEmptyResults);

        public WaitForCaseEmptyResults(ILogger<WaitForCaseEmptyResults> log, ISearchIndexService searchIndexService, IJsonConvertWrapper jsonConvertWrapper, ITelemetryAugmentationWrapper telemetryAugmentationWrapper, IExceptionHandler exceptionHandler)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        [FunctionName(nameof(WaitForCaseEmptyResults))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.WaitForCaseEmptyResults)] HttpRequestMessage request)
        {
            Guid correlationId = default;

            try
            {
                correlationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

                if (request.Content == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }
                var content = await request.Content.ReadAsStringAsync();
                var requestDto = _jsonConvertWrapper.DeserializeObject<WaitForCaseEmptyResultsRequestDto>(content);

                _log.LogMethodFlow(correlationId, loggingName, $"Begin check case is empty for case ID {requestDto.CaseId}");

                var result = await _searchIndexService.WaitForCaseEmptyResultsAsync(requestDto.CaseId);

                _log.LogMethodFlow(correlationId, loggingName, $"Case is empty check completed for case ID {requestDto.CaseId}");

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