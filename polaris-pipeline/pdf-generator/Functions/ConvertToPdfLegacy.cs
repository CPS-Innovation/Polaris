using System;
using System.Net;
using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using pdf_generator.Services.PdfService;
using pdf_generator.TelemetryEvents;
using Common.Exceptions;
using pdf_generator.Extensions;
using Common.Extensions;
using Common.Logging;
using Common.Telemetry;
using Common.Streaming;
using System.Threading.Tasks;
using Common.Constants;

namespace pdf_generator.Functions
{
    public class ConvertToPdfLegacy
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly ILogger<ConvertToPdfLegacy> _logger;
        private const string LoggingName = nameof(ConvertToPdfLegacy);

        public ConvertToPdfLegacy(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<ConvertToPdfLegacy> logger)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
            _logger = logger;
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.UnsupportedMediaType)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Function(nameof(ConvertToPdfLegacy))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ConvertToPdfLegacy)] HttpRequest request,
            string caseUrn, int caseId, string materialId, string documentId)
        {
            Guid currentCorrelationId = default;
            currentCorrelationId = request.Headers.GetCorrelationId();

            var telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId)
            {
                OperationName = nameof(ConvertToPdfLegacy),
            };
            try
            {
                var fileType = ConvertToPdfHelper.GetFileType(request.Headers);

                telemetryEvent.FileType = fileType.ToString();
                telemetryEvent.CaseId = caseId.ToString();
                telemetryEvent.CaseUrn = caseUrn;
                telemetryEvent.DocumentId = materialId;
                telemetryEvent.VersionId = documentId;

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

                var conversionResult = await _pdfOrchestratorService.ReadToPdfStreamAsync(inputStream, fileType, materialId, currentCorrelationId);

                // #25834 - Successfully converted documents may still have a failure reason we need to record
                if (conversionResult.HasFailureReason())
                {
                    telemetryEvent.FailureReason = conversionResult.GetFailureReason();
                }

                if (conversionResult.ConversionStatus == PdfConversionStatus.DocumentConverted)
                {
                    var bytes = conversionResult.ConvertedDocument.Length;

                    telemetryEvent.Bytes = bytes;
                    telemetryEvent.EndTime = DateTime.UtcNow;
                    telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();

                    _logger.TrackEvent(telemetryEvent);

                    return new FileStreamResult(conversionResult.ConvertedDocument, "application/pdf")
                    {
                        FileDownloadName = $"{nameof(ConvertToPdfLegacy)}.pdf",
                    };
                }

                telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();
                _logger.TrackEventFailure(telemetryEvent);

                return new ObjectResult(conversionResult.ConversionStatus)
                {
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);

                telemetryEvent.FailureReason = exception.Message;
                _logger.TrackEventFailure(telemetryEvent);

                return new ObjectResult(exception.ToFormattedString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }


    }
}