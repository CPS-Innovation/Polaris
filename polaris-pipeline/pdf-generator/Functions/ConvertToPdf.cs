using Common.Configuration;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Logging;
using Common.Streaming;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using pdf_generator.Extensions;
using pdf_generator.Models;
using pdf_generator.Services;
using pdf_generator.TelemetryEvents;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace pdf_generator.Functions
{
    public class ConvertToPdf
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly ILogger<ConvertToPdf> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private const string LoggingName = nameof(ConvertToPdf);

        public ConvertToPdf(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<ConvertToPdf> logger,
             ITelemetryClient telemetryClient)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.UnsupportedMediaType)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Function(nameof(ConvertToPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ConvertToPdf)] HttpRequest request,
            string caseUrn, int caseId, string documentId, long versionId)
        {
            Guid currentCorrelationId = default;
            currentCorrelationId = request.Headers.GetCorrelationId();
            var cmsAuthValues = request.Headers.GetCmsAuthValues();

            var telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId)
            {
                OperationName = nameof(ConvertToPdf),
            };
            try
            {
                var fileType = ConvertToPdfHelper.GetFileType(request.Headers);

                telemetryEvent.FileType = fileType.ToString();
                telemetryEvent.CaseId = caseId.ToString();
                telemetryEvent.CaseUrn = caseUrn;
                telemetryEvent.DocumentId = documentId;
                telemetryEvent.VersionId = versionId.ToString();

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

                var readToPdfDto = new ReadToPdfDto
                {
                    Urn = caseUrn,
                    CaseId = caseId,
                    DocumentId = documentId,
                    VersionId = versionId,
                    FileType = fileType,
                    Stream = inputStream,
                    CorrelationId = currentCorrelationId,
                    CmsAuthValues = cmsAuthValues
                };

                var conversionResult = await _pdfOrchestratorService.ReadToPdfStreamAsync(readToPdfDto);

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

                    _telemetryClient.TrackEvent(telemetryEvent);

                    return new FileStreamResult(conversionResult.ConvertedDocument, "application/pdf")
                    {
                        FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                    };
                }

                telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();
                _telemetryClient.TrackEventFailure(telemetryEvent);

                return new ObjectResult(conversionResult.ConversionStatus)
                {
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);

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