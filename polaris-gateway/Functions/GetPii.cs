using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Handlers;
using Microsoft.AspNetCore.Http.Extensions;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;


namespace PolarisGateway.Functions
{
    public class GetPii
    {
        private const string JsonContentType = "application/json";
        private const string tokenQueryParamName = "token";
        private const string isOcrProcessedParamName = "isOcrProcessed";
        private readonly ILogger<GetPii> _logger;
        private readonly IPiiArtefactService _piiArtefactService;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetPii(
            ILogger<GetPii> logger,
            IPiiArtefactService piiArtefactService,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _piiArtefactService = piiArtefactService ?? throw new ArgumentNullException(nameof(piiArtefactService));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
        }

        [FunctionName(nameof(GetPii))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pii)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);

                var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
                var token = req.Query.ContainsKey(tokenQueryParamName)
                    ? Guid.Parse(req.Query[tokenQueryParamName])
                    : (Guid?)null;

                var ocrResult = await _piiArtefactService.GetPiiAsync(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token);
                return ocrResult.Status switch
                {
                    ResultStatus.ArtefactAvailable => new JsonResult(ocrResult.Artefact),
                    ResultStatus.PollWithToken => new JsonResult(new
                    {
                        NextUrl = $"{req.GetDisplayUrl()}{(req.QueryString.Value.StartsWith("?") ? "&" : "?")}{tokenQueryParamName}={ocrResult.ContinuationToken}"
                    })
                    {
                        StatusCode = (int)HttpStatusCode.Accepted // the client will understand 202 as a signal to poll again
                    },
                    ResultStatus.Failed => new JsonResult(ocrResult)
                    {
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                    },
                    _ => new JsonResult(ocrResult) { StatusCode = (int)HttpStatusCode.InternalServerError },
                };
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                  _logger,
                  nameof(GetPii),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}

