using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Handlers;
using PolarisGateway.Services;


namespace PolarisGateway.Functions
{
    public class GetPdf
    {
        private const string PdfContentType = "application/pdf";
        private readonly ILogger<GetPdf> _logger;
        private readonly IArtefactService _artefactService;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetPdf(
            ILogger<GetPdf> logger,
            IArtefactService artefactService,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _artefactService = artefactService;
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

                var isOcrProcessed = req.Query.ContainsKey("isOcrProcessed") && bool.Parse(req.Query["isOcrProcessed"]);
                var pdfStream = await _artefactService.GetPdf(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, versionId, isOcrProcessed);
                if (pdfStream != null)
                {
                    return new FileStreamResult(pdfStream, PdfContentType);
                }

                return new NotFoundResult();
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

