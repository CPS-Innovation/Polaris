using Common.Configuration;
using Common.Extensions;
using Common.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    public class DocumentIndexCountLegacy : BaseFunction
    {
        private readonly ILogger<DocumentIndexCountLegacy> _log;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;
        private const string LoggingName = nameof(DocumentIndexCountLegacy);

        public DocumentIndexCountLegacy(
            ILogger<DocumentIndexCountLegacy> log,
            ISearchIndexService searchIndexService,
            IExceptionHandler exceptionHandler)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        [Function(nameof(DocumentIndexCountLegacy))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentIndexCountLegacy)] HttpRequest request, int caseId, string materialId, long documentId)
        {
            var correlationId = Guid.Empty;

            try
            {
                correlationId = request.Headers.GetCorrelationId();

                var result = await _searchIndexService.GetDocumentIndexCount(caseId, materialId, documentId, correlationId);

                return CreateJsonResult(result);
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleExceptionNew(exception, correlationId, LoggingName, _log);
            }
        }
    }
}