using System;
using System.IO;
using System.Net;
using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using pdf_generator.Services.PdfService;
using pdf_generator.TelemetryEvents;
using Common.Domain.Exceptions;
using Common.Extensions;
using Common.Logging;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Microsoft.AspNetCore.Http.Features;

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

                    var pdfStream = _pdfOrchestratorService.ReadToPdfStream(request.Body, fileType, documentId, currentCorrelationId);
                    var bytes = pdfStream.Length;

                    telemetryEvent.Bytes = bytes;
                    telemetryEvent.EndTime = DateTime.UtcNow;

                    _telemetryClient.TrackEvent(telemetryEvent);

                    pdfStream.Position = 0;
                    return new FileStreamResult(pdfStream, "application/pdf")
                    {
                        FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                    };
                }
                else
                {
                    throw new BadRequestException("An empty document stream was received from the Coordinator", nameof(request));
                }
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);

                return new ObjectResult(exception.ToString())
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