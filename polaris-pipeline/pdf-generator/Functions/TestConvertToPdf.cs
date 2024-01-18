/*using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using pdf_generator.Services.PdfService;
using Common.Domain.Exceptions;
using Common.Extensions;
using Common.Logging;

namespace pdf_generator.Functions
{
    public class TestConvertToPdf
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly ILogger<TestConvertToPdf> _logger;
        private const string LoggingName = nameof(TestConvertToPdf);

        public TestConvertToPdf(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<TestConvertToPdf> logger)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
            _logger = logger;
        }

        [Function(nameof(TestConvertToPdf))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "test-convert-to-pdf")] HttpRequest request, 
            FunctionContext executionContext)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                #region Validate-Inputs
                
                var fileType = request.Headers.GetFileType();
                
                const string documentId = "test-document";
                
                #endregion
                
                request.EnableBuffering();
                if (request.ContentLength == null || !request.Body.CanSeek)
                    throw new BadRequestException("An empty document stream was received from the TestClient", nameof(request));
                
                var originalBytes = request.ContentLength;
                _logger.LogMethodFlow(correlationId, LoggingName, $"Original bytes received: {originalBytes}");
                
                request.Body.Seek(0, SeekOrigin.Begin);
                    
                var pdfStream = _pdfOrchestratorService.ReadToPdfStream(request.Body, fileType, documentId, Guid.NewGuid());
                var bytes = pdfStream.Length;
                _logger.LogMethodFlow(correlationId, LoggingName, $"Converted bytes: {bytes}");
                    
                pdfStream.Position = 0;
                return new FileStreamResult(pdfStream, "application/pdf")
                {
                    FileDownloadName = $"{nameof(TestConvertToPdf)}.pdf",
                };
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(correlationId, LoggingName, exception.Message, exception);
 
                return new ObjectResult(exception.ToString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
    }
}*/