using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Handlers;
using Common.Services.BlobStorageService;
using Common.Helpers;
using Ddei;
using Ddei.Factories;
using System.Text.RegularExpressions;

namespace PolarisGateway.Functions
{
    public class GetPdf
    {
        private const string PdfContentType = "application/pdf";
        private readonly ILogger<GetPdf> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IV2PolarisBlobStorageService _blobStorageService;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetPdf(
            ILogger<GetPdf> logger,
            IV2PolarisBlobStorageService blobStorageService,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _blobStorageService = blobStorageService;
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

                var blobName = BlobNameHelper.GetBlobName(caseId, documentId, versionId, BlobNameHelper.BlobType.Pdf);
                var metaData = new Dictionary<string, string> { { "isOcrProcessed", isOcrProcessed.ToString() } };

                var blobStream = await _blobStorageService.GetDocumentAsync(blobName, metaData);
                if (blobStream != null)
                {
                    return new FileStreamResult(blobStream, PdfContentType);
                }

                var documentIdWithoutPrefix = long.Parse(Regex.Match(documentId, @"\d+").Value);
                var ddeiArgs = _ddeiArgFactory.CreateDocumentArgDto(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentIdWithoutPrefix, versionId);

                var documentStream = await _ddeiClient.GetDocumentAsync(ddeiArgs);
                await _blobStorageService.UploadDocumentAsync(documentStream, blobName, metaData);
                var blobStream2 = await _blobStorageService.GetDocumentAsync(blobName, metaData);
                if (blobStream2 != null)
                {
                    return new FileStreamResult(blobStream2, PdfContentType);
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

