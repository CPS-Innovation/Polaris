using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
using Common.Streaming;
using Common.Constants;
using System.Linq;
using Common.Domain.Document;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.ConvertToPdf)] HttpRequestMessage request,
            string caseUrn, string caseId, string documentId, string versionId)
        {
            Guid currentCorrelationId = default;
            ConvertedDocumentEvent telemetryEvent = default;
            try
            {
                #region Validate-Inputs

                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId);

                request.Headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
                if (cmsAuthValuesValues == null)
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));
                var cmsAuthValues = cmsAuthValuesValues.First();
                if (string.IsNullOrWhiteSpace(cmsAuthValues))
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));


                request.Headers.TryGetValues(HttpHeaderKeys.Filetype, out var filetypes);
                if (filetypes == null)
                    throw new BadRequestException("Missing Filetype Value", nameof(request));
                var filetypeValue = filetypes.First();
                if (string.IsNullOrEmpty(filetypeValue))
                    throw new BadRequestException("Null Filetype Value", filetypeValue);
                if (!Enum.TryParse(filetypeValue, true, out FileType filetype))
                    throw new BadRequestException("Invalid Filetype Enum Value", filetypeValue);
                telemetryEvent.FileType = filetype.ToString();

                telemetryEvent.CaseId = caseId;
                telemetryEvent.CaseUrn = caseUrn;

                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                telemetryEvent.DocumentId = documentId;

                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId);
                telemetryEvent.VersionId = versionId;

                #endregion

                var startTime = DateTime.UtcNow;
                telemetryEvent.StartTime = startTime;

                if (request.Content == null)
                {
                    throw new BadRequestException("An empty document stream was received from the Coordinator", nameof(request));
                }

                var inputStream = await request.Content
                    .ReadAsStreamAsync()
                    // Aspose demands a seekable stream, and as we want to record the size of the stream, we need to ensure it is seekable also.
                    .EnsureSeekableAsync();

                var originalBytes = inputStream.Length;
                telemetryEvent.OriginalBytes = originalBytes;

                var pdfStream = _pdfOrchestratorService.ReadToPdfStream(inputStream, filetype, documentId, currentCorrelationId);
                var bytes = pdfStream.Length;

                telemetryEvent.Bytes = bytes;
                telemetryEvent.EndTime = DateTime.UtcNow;

                _telemetryClient.TrackEvent(telemetryEvent);

                return new FileStreamResult(pdfStream, "application/pdf")
                {
                    FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                };
            }
            catch (Exception exception)
            {
                _logger.LogMethodError(currentCorrelationId, nameof(ConvertToPdf), exception.Message, exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);

                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}