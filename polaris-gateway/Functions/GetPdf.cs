using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Handlers;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;


namespace PolarisGateway.Functions
{
    public class GetPdf
    {
        private const string PdfContentType = "application/pdf";
        private const string isOcrProcessedParamName = "isOcrProcessed";
        private readonly ILogger<GetPdf> _logger;
        private readonly IArtefactService _cachingArtefactService;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetPdf(
            ILogger<GetPdf> logger,
            ICachingArtefactService cachingArtefactService,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _cachingArtefactService = cachingArtefactService;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
        }

        [FunctionName(nameof(GetPdf))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pdf)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);

                var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
                var getPdfResult = await _cachingArtefactService.GetPdfAsync(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, versionId, isOcrProcessed);
                return getPdfResult.Status == ResultStatus.ArtefactAvailable
                    ? new FileStreamResult(getPdfResult.Result, PdfContentType)
                    : new JsonResult(getPdfResult)
                    {
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                    };
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                  _logger,
                  nameof(GetPdf),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}
