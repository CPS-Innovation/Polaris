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
    public class GetOcr
    {
        private const string JsonContentType = "application/json";
        private const string tokenQueryParamName = "token";
        private const string isOcrProcessedParamName = "isOcrProcessed";
        private readonly ILogger<GetOcr> _logger;
        private readonly ICachingArtefactService _cachingArtefactService;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetOcr(
            ILogger<GetOcr> logger,
            ICachingArtefactService artefactService,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachingArtefactService = artefactService ?? throw new ArgumentNullException(nameof(artefactService));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
        }

        [FunctionName(nameof(GetOcr))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Ocr)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);

                var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
                var token = req.Query.ContainsKey(tokenQueryParamName)
                    ? Guid.Parse(req.Query[tokenQueryParamName])
                    : (Guid?)null;

                var ocrResult = await _cachingArtefactService.GetOcrAsync(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token);
                return ocrResult.Status switch
                {
                    ResultStatus.ArtefactAvailable => new JsonResult(
                        ocrResult.Result.Item2
                    )
                    {
                        StatusCode = (int)HttpStatusCode.OK
                    },
                    ResultStatus.PollWithToken => new JsonResult(new
                    {
                        NextUrl = $"{req.GetDisplayUrl()}{(req.QueryString.Value.StartsWith("?") ? "&" : "?")}{tokenQueryParamName}={ocrResult.Result.Item1}"
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
                  nameof(GetOcr),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}

