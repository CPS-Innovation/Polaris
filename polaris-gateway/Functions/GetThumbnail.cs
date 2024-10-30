using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.PdfThumbnailGenerator;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{ 
    public class GetThumbnail 
    {
        private readonly ILogger<GetThumbnail> _logger;
        private readonly IPdfThumbnailGeneratorClient _pdfThumbnailGeneratorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetThumbnail(ILogger<GetThumbnail> logger, IPdfThumbnailGeneratorClient pdfThumbnailGeneratorClient, IInitializationHandler initializationHandler, IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
            _pdfThumbnailGeneratorClient = pdfThumbnailGeneratorClient ?? throw new ArgumentNullException(nameof(pdfThumbnailGeneratorClient)); 
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler)); 
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
        }
        
        [FunctionName(nameof(GetThumbnail))]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Thumbnail)] HttpRequest req, 
            string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int pageIndex) 
        { 
            (Guid CorrelationId, string CmsAuthValues) context = default; 
            try 
            { 
                context = await _initializationHandler.Initialize(req); 
                return await _pdfThumbnailGeneratorClient.GetThumbnailAsync(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex, context.CmsAuthValues, context.CorrelationId);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledException(_logger, nameof(GetThumbnail), context.CorrelationId, ex);
            }
        }
    }
}