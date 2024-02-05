using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Common.Domain.Document;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using pdf_generator.Services.PdfService;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Extensions;
using Common.Logging;
using Common.Streaming;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using pdf_generator.TelemetryEvents;

namespace pdf_generator.Functions
{
    [ExcludeFromCodeCoverage]
    public class TestConvertToPdf
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly ILogger<TestConvertToPdf> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private const string LoggingName = nameof(TestConvertToPdf);

        public TestConvertToPdf(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<TestConvertToPdf> logger,
             ITelemetryClient telemetryClient,
             ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        [Function(nameof(TestConvertToPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/test-convert-to-pdf")] HttpRequest request, 
            string caseUrn, string caseId, string documentId, string versionId, FunctionContext executionContext)
        {
            Guid currentCorrelationId = default;
            ConvertedDocumentEvent telemetryEvent = default;
            try
            {
                #region Validate-Inputs
                
                currentCorrelationId = request.Headers.GetCorrelation();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);
                telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId);
                _logger.LogMethodEntry(currentCorrelationId, LoggingName, string.Empty);

                //request.Headers.CheckForCmsAuthValues();

                var fileType = request.Headers.GetFileType();
                telemetryEvent.FileType = fileType.ToString();
                telemetryEvent.CaseId = caseId;

                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                telemetryEvent.DocumentId = documentId;

                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId);
                telemetryEvent.VersionId = versionId;

                #endregion
                
                var startTime = DateTime.UtcNow;
                telemetryEvent.StartTime = startTime;
                
                if (request.Body == null)
                {
                    throw new BadRequestException("An empty document stream was received from the Coordinator", nameof(request));
                }

                var inputStream = await request.Body
                    // Aspose demands a seekable stream, and as we want to record the size of the stream, we need to ensure it is seekable also.
                    .EnsureSeekableAsync();

                var originalBytes = inputStream.Length;
                telemetryEvent.OriginalBytes = originalBytes;

                var conversionResult = _pdfOrchestratorService.ReadToPdfStream(inputStream, fileType, documentId, currentCorrelationId);
                if (conversionResult.ConversionStatus == PdfConversionStatus.DocumentConverted)
                {
                    var bytes = conversionResult.ConvertedDocument.Length;

                    telemetryEvent.Bytes = bytes;
                    telemetryEvent.EndTime = DateTime.UtcNow;
                    telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();

                    _telemetryClient.TrackEvent(telemetryEvent);

                    return new FileStreamResult(conversionResult.ConvertedDocument, "application/pdf")
                    {
                        FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                    };
                }

                var failureReason = conversionResult.GetFailureReason();
                telemetryEvent.FailureReason = failureReason;
                telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();
                _telemetryClient.TrackEventFailure(telemetryEvent);
                    
                return new ObjectResult(failureReason)
                {
                    StatusCode = (int) HttpStatusCode.InternalServerError
                };
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);

                if (telemetryEvent == null)
                    return new ObjectResult(exception.ToFormattedString())
                    {
                        StatusCode = (int) HttpStatusCode.InternalServerError
                    };
                
                telemetryEvent.FailureReason = exception.Message;
                _telemetryClient.TrackEventFailure(telemetryEvent);

                return new ObjectResult(exception.ToFormattedString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
    }
}