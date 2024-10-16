using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Handlers;
using PolarisGateway.Services;
using PolarisGateway.Services.Domain;
using Microsoft.AspNetCore.Http.Extensions;


namespace PolarisGateway.Functions
{
    public class GetOcr
    {
        private const string JsonContentType = "application/json";
        private const string tokenQueryParamName = "token";
        private const string isOcrProcessedParamName = "isOcrProcessed";
        private readonly ILogger<GetOcr> _logger;
        private readonly IArtefactService _artefactService;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetOcr(
            ILogger<GetOcr> logger,
            IArtefactService artefactService,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _artefactService = artefactService;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
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

                var ocrResult = await _artefactService.GetOcr(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token);
                return ocrResult.Status switch
                {
                    JsonArtefactResult.ResultStatus.ArtefactAvailable => new FileStreamResult(ocrResult.Stream, JsonContentType),
                    JsonArtefactResult.ResultStatus.PollWithToken => new JsonResult(new
                    {
                        ocrResult.Token,
                        NextUrl = $"{req.GetDisplayUrl()}{(req.QueryString.Value.StartsWith("?") ? "&" : "?")}{tokenQueryParamName}={ocrResult.Token}"
                    })
                    {
                        StatusCode = (int)HttpStatusCode.Accepted
                    },
                    JsonArtefactResult.ResultStatus.FailedOnPdfConversion
                        => new JsonResult(ocrResult)
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

