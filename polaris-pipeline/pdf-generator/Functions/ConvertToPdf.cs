using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Extensions;
using Common.Logging;
using Common.Streaming;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using pdf_generator.Services.PdfService;
using pdf_generator.TelemetryEvents;

namespace pdf_generator.Functions
{
    public class ConvertToPdf
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly ILogger<ConvertToPdf> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        const string LoggingName = nameof(ConvertToPdf);

        public ConvertToPdf(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<ConvertToPdf> logger,
             ITelemetryClient telemetryClient,
             ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _pdfOrchestratorService = pdfOrchestratorService ?? throw new ArgumentNullException(nameof(pdfOrchestratorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
        }

        [FunctionName(nameof(ConvertToPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.ConvertToPdf)] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            ConvertedDocumentEvent telemetryEvent = default;
            try
            {
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

                request.Headers.TryGetValues(HttpHeaderKeys.CaseId, out var caseIds);
                if (caseIds == null)
                    throw new BadRequestException("Missing CaseIds", nameof(request));
                var caseId = caseIds.First();
                if (string.IsNullOrEmpty(caseId))
                    throw new BadRequestException("Invalid CaseId", caseId);
                telemetryEvent.CaseId = caseId;

                request.Headers.TryGetValues(HttpHeaderKeys.DocumentId, out var documentIds);
                if (documentIds == null)
                    throw new BadRequestException("Missing DocumentIds", nameof(request));
                var documentId = documentIds.First();
                if (string.IsNullOrEmpty(documentId))
                    throw new BadRequestException("Invalid DocumentId", documentId);
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                telemetryEvent.DocumentId = documentId;

                request.Headers.TryGetValues(HttpHeaderKeys.VersionId, out var versionIds);
                if (versionIds == null)
                    throw new BadRequestException("Missing VersionIds", nameof(request));
                var versionId = versionIds.First();
                if (string.IsNullOrEmpty(versionId))
                    throw new BadRequestException("Invalid VersionId", versionId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(versionId);
                telemetryEvent.VersionId = versionId;

                var startTime = DateTime.UtcNow;
                telemetryEvent.StartTime = startTime;

                if (request.Content == null)
                {
                    throw new BadRequestException("An empty document stream was received from the Coordinator", nameof(request));
                }

                var inputStream = await request.Content
                    .ReadAsStreamAsync()
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
                _logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);

                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}