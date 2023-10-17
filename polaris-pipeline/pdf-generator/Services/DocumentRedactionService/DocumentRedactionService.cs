using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Facades;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Telemetry.Contracts;
using Microsoft.Extensions.Logging;
using pdf_generator.TelemetryEvents;
using pdf_generator.TelemetryEvents.Extensions;
using pdf_generator.Services.DocumentRedactionService.RedactionImplementation;

namespace pdf_generator.Services.DocumentRedactionService
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ICoordinateCalculator _coordinateCalculator;
        private readonly IRedactionImplementation _redactionImplementation;
        private readonly ILogger<DocumentRedactionService> _logger;
        private readonly ITelemetryClient _telemetryClient;

        public DocumentRedactionService(
            IPolarisBlobStorageService blobStorageService,
            ICoordinateCalculator coordinateCalculator,
            IRedactionImplementation redactionImplementation,
            ILogger<DocumentRedactionService> logger,
            ITelemetryClient telemetryClient)
        {
            _polarisBlobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _coordinateCalculator = coordinateCalculator ?? throw new ArgumentNullException(nameof(coordinateCalculator));
            _redactionImplementation = redactionImplementation;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            RedactedDocumentEvent telemetryEvent = default;
            try
            {
                telemetryEvent = new RedactedDocumentEvent(correlationId: correlationId,
                    caseId: redactPdfRequest.CaseId.ToString(),
                    documentId: redactPdfRequest.PolarisDocumentIdValue,
                    redactionPageCounts: redactPdfRequest.RedactionPageCounts(),
                    implementationType: _redactionImplementation.GetImplementationType());

                _logger.LogMethodEntry(correlationId, nameof(RedactPdfAsync), redactPdfRequest.ToJson());

                var fileName = redactPdfRequest.FileName;
                var documentBlob = await _polarisBlobStorageService.GetDocumentAsync(fileName, correlationId);
                if (documentBlob == null)
                {
                    return new RedactPdfResponse
                    {
                        Succeeded = false,
                        Message = $"Invalid document - a document with filename '{fileName}' could not be retrieved for redaction purposes"
                    };
                }
                telemetryEvent.StartTime = DateTime.UtcNow;
                telemetryEvent.OriginalBytes = documentBlob.Length;

                using var document = new Document(documentBlob);
                AddAnnotations(document, redactPdfRequest, correlationId);

                Document sanitizedDocument;
                try
                {
                    sanitizedDocument = _redactionImplementation.SanitizeDocument(document);
                }
                catch (Exception ex)
                {
                    _logger.LogMethodError(correlationId, nameof(RedactPdfAsync), "Could not sanitize document", ex);
                    sanitizedDocument = document;
                }

                using var redactedDocumentStream = SaveToStream(sanitizedDocument);

                telemetryEvent.Bytes = redactedDocumentStream.Length;
                telemetryEvent.EndTime = DateTime.UtcNow;

                var uploadFileName = GetUploadFileName(fileName);
                await _polarisBlobStorageService.UploadDocumentAsync(redactedDocumentStream, uploadFileName, redactPdfRequest.CaseId.ToString(), redactPdfRequest.PolarisDocumentId, redactPdfRequest.VersionId.ToString(), correlationId);

                _telemetryClient.TrackEvent(telemetryEvent);

                return new RedactPdfResponse
                {
                    Succeeded = true,
                    RedactedDocumentName = uploadFileName
                };
            }
            catch (Exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }

        private void AddAnnotations(Document document, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            var pdfInfo = new PdfFileInfo(document);

            foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
            {
                var currentPage = redactionPage.PageIndex;
                var annotationPage = document.Pages[currentPage];

                foreach (var boxToDraw in redactionPage.RedactionCoordinates)
                {
                    var translatedCoordinates = _coordinateCalculator.CalculateRelativeCoordinates(redactionPage.Width,
                        redactionPage.Height, currentPage, boxToDraw, pdfInfo, correlationId);

                    var annotationRectangle = new Rectangle(
                        translatedCoordinates.X1,
                        translatedCoordinates.Y1,
                        translatedCoordinates.X2,
                        translatedCoordinates.Y2);

                    _redactionImplementation.AttachAnnotation(annotationPage, annotationRectangle);
                }
            }
        }

        private Stream SaveToStream(Document inputDocument)
        {
            var stream = new MemoryStream();
            inputDocument.Save(stream);
            return stream;
        }

        private string GetUploadFileName(string fileName)
        {
            var fileNameWithoutExtension = fileName.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) > -1 ? fileName.Split(".pdf", StringSplitOptions.RemoveEmptyEntries)[0] : fileName;
            return $"{fileNameWithoutExtension}_{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.pdf"; //restore save redaction to same storage for now, but with additional randomised identifier
        }
    }
}