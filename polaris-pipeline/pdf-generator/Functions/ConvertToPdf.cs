using System;
using System.IO;
using System.Net;
using Common.Configuration;
using Common.Domain.Document;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using pdf_generator.Services.PdfService;
using pdf_generator.TelemetryEvents;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Extensions;
using Common.Logging;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers.Contracts;

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

        [Function(nameof(ConvertToPdf))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.ConvertToPdf)] HttpRequest request,
            string caseUrn, string caseId, string documentId, string versionId)
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

                request.Headers.CheckForCmsAuthValues();

                var fileType = request.Headers.GetFileType();
                telemetryEvent.FileType = fileType.ToString();
                telemetryEvent.CaseId = caseId;
                telemetryEvent.CaseUrn = caseUrn;

                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                telemetryEvent.DocumentId = documentId;

                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId);
                telemetryEvent.VersionId = versionId;

                #endregion

                var startTime = DateTime.UtcNow;
                telemetryEvent.StartTime = startTime;

                request.EnableBuffering();
                if (request.ContentLength != null && request.Body.CanSeek)
                {
                    var originalBytes = request.ContentLength;
                    telemetryEvent.OriginalBytes = originalBytes.Value;

                    request.Body.Seek(0, SeekOrigin.Begin);
                    
                    var conversionResult = _pdfOrchestratorService.ReadToPdfStream(request.Body, fileType, documentId, currentCorrelationId);
                    if (conversionResult.ConversionStatus == PdfConversionStatus.DocumentConverted)
                    {
                        var bytes = conversionResult.ConvertedDocument.Length;

                        telemetryEvent.Bytes = bytes;
                        telemetryEvent.EndTime = DateTime.UtcNow;
                        telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();

                        _telemetryClient.TrackEvent(telemetryEvent);

                        conversionResult.ConvertedDocument.Position = 0;
                        return new FileStreamResult(conversionResult.ConvertedDocument, "application/pdf")
                        {
                            FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                        };
                    }
                    else
                    {
                        var failureReason = conversionResult.GetFailureReason();
                        telemetryEvent.FailureReason = failureReason;
                        telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();
                        _telemetryClient.TrackEventFailure(telemetryEvent);
                        
                        return new ObjectResult(failureReason)
                        {
                            StatusCode = (int) HttpStatusCode.InternalServerError
                        };
                    }
                }
                else
                {
                    throw new BadRequestException("An empty document stream was received from the Coordinator", nameof(request));
                }
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);

                if (telemetryEvent != null)
                {
                    telemetryEvent.FailureReason = exception.Message;
                    _telemetryClient.TrackEventFailure(telemetryEvent);
                }

                return new ObjectResult(exception.ToFormattedString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, LoggingName, nameof(ConvertToPdf));
            }
        }
    }
}