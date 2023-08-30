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
        private readonly ILogger<ConvertToPdf> _log;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        const string loggingName = nameof(ConvertToPdf);

        public ConvertToPdf(
             IPdfOrchestratorService pdfOrchestratorService,
             ILogger<ConvertToPdf> logger,
             ITelemetryClient telemetryClient,
             ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
            _log = logger;
            _telemetryClient = telemetryClient;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        [FunctionName(nameof(ConvertToPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.ConvertToPdf)] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            ConvertedDocumentEvent telemetryEvent = default;
            try
            {
                #region Validate-Inputs        
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);
                telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId);
                _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

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

                #endregion

                var startTime = DateTime.UtcNow;
                telemetryEvent.StartTime = startTime;

                var inputStream = await request.Content.ReadAsStreamAsync();
                var originalBytes = inputStream.Length;
                telemetryEvent.OriginalBytes = originalBytes;

                var pdfStream = _pdfOrchestratorService.ReadToPdfStream(inputStream, filetype, documentId, currentCorrelationId);
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
            catch (Exception exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);

                return new ObjectResult(exception.ToString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, nameof(ConvertToPdf));
            }
        }
    }
}