using Common.Configuration;
using Common.Extensions;
using Common.Handlers;
using text_extractor.Services.CaseSearchService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace text_extractor.Functions
{
    public class RemoveCaseIndexes : BaseFunction
    {
        private readonly ILogger<RemoveCaseIndexes> _log;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;
        private const string LoggingName = nameof(RemoveCaseIndexes);

        public RemoveCaseIndexes(
            ILogger<RemoveCaseIndexes> logger,
            ISearchIndexService searchIndexService,
            IExceptionHandler exceptionHandler)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        [Function(nameof(RemoveCaseIndexes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.RemoveCaseIndexes)] HttpRequest request, int caseId)
        {
            Guid correlationId = default;

            try
            {
                correlationId = request.Headers.GetCorrelationId();

                var result = await _searchIndexService.RemoveCaseIndexEntriesAsync(caseId, correlationId);

                return CreateJsonResult(result);
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleExceptionNew(exception, correlationId, LoggingName, _log);
            }
        }
    }
}