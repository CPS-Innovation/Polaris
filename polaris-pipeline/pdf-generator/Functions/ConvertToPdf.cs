using System;
using System.IO;
using System.Linq;
using System.Net;
using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using pdf_generator.Services.PdfService;
using pdf_generator.TelemetryEvents;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using polaris_common.Extensions;

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
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.ConvertToPdf)] HttpRequest request)
        {
            Guid currentCorrelationId = default;
            ConvertedDocumentEvent telemetryEvent = default;
            try
            {
                #region Validate-Inputs
                
                currentCorrelationId = request.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);
                telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId);
                _logger.LogMethodEntry(currentCorrelationId, LoggingName, string.Empty);

                var cmsAuthValuesReceived = request.Headers.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
                if (!cmsAuthValuesReceived)
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));
                var cmsAuthValues = cmsAuthValuesValues.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(cmsAuthValues))
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));

                var fileTypeReceived = request.Headers.TryGetValue(HttpHeaderKeys.Filetype, out var filetypes);
                if (!fileTypeReceived)
                    throw new BadRequestException("Missing Filetype Value", nameof(request));
                var filetypeValue = filetypes.FirstOrDefault();
                if (string.IsNullOrEmpty(filetypeValue))
                    throw new BadRequestException("Null Filetype Value", filetypeValue);
                if (!Enum.TryParse(filetypeValue, true, out FileType filetype))
                    throw new BadRequestException("Invalid Filetype Enum Value", filetypeValue);
                telemetryEvent.FileType = filetype.ToString();

                var caseIdReceived = request.Headers.TryGetValue(HttpHeaderKeys.CaseId, out var caseIds);
                if (!caseIdReceived)
                    throw new BadRequestException("Missing CaseIds", nameof(request));
                var caseId = caseIds.FirstOrDefault();
                if (string.IsNullOrEmpty(caseId))
                    throw new BadRequestException("Invalid CaseId", caseId);
                telemetryEvent.CaseId = caseId;

                var documentIdReceived = request.Headers.TryGetValue(HttpHeaderKeys.DocumentId, out var documentIds);
                if (!documentIdReceived)
                    throw new BadRequestException("Missing DocumentIds", nameof(request));
                var documentId = documentIds.FirstOrDefault();
                if (string.IsNullOrEmpty(documentId))
                    throw new BadRequestException("Invalid DocumentId", documentId);
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                telemetryEvent.DocumentId = documentId;

                var versionIdReceived = request.Headers.TryGetValue(HttpHeaderKeys.VersionId, out var versionIds);
                if (!versionIdReceived)
                    throw new BadRequestException("Missing VersionIds", nameof(request));
                var versionId = versionIds.FirstOrDefault();
                if (string.IsNullOrEmpty(versionId))
                    throw new BadRequestException("Invalid VersionId", versionId);
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
                    
                    var pdfStream = _pdfOrchestratorService.ReadToPdfStream(request.Body, filetype, documentId, currentCorrelationId);
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