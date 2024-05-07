﻿using System;
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
    public class ConvertToPdf
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly ILogger<ConvertToPdf> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private const string LoggingName = nameof(ConvertToPdf);

        public ConvertToPdf(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<ConvertToPdf> logger,
             ITelemetryClient telemetryClient,
             ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.UnsupportedMediaType)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Function(nameof(ConvertToPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ConvertToPdf)] HttpRequest request,
            string caseUrn, string caseId, string documentId, string versionId)
        {
            Guid currentCorrelationId = default;
            ConvertedDocumentEvent telemetryEvent = default;
            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId);

                var fileType = ConvertToPdfHelper.GetFileType(request.Headers);

                telemetryEvent.FileType = fileType.ToString();

                telemetryEvent.CaseId = caseId;
                telemetryEvent.CaseUrn = caseUrn;

                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                telemetryEvent.DocumentId = documentId;

                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId);
                telemetryEvent.VersionId = versionId;

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

                if (telemetryEvent == null)
                    return new ObjectResult(exception.ToFormattedString())
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
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