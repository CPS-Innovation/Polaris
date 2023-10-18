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
using pdf_generator.Services.DocumentRedactionService.RedactionProvider;
using Aspose.Pdf.Text;
using System.Linq;

namespace pdf_generator.Services.DocumentRedactionService
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ICoordinateCalculator _coordinateCalculator;
        private readonly IRedactionProvider _redactionProvider;
        private readonly ILogger<DocumentRedactionService> _logger;
        private readonly ITelemetryClient _telemetryClient;

        public DocumentRedactionService(
            IPolarisBlobStorageService blobStorageService,
            ICoordinateCalculator coordinateCalculator,
            IRedactionProvider redactionProvider,
            ILogger<DocumentRedactionService> logger,
            ITelemetryClient telemetryClient)
        {
            _polarisBlobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _coordinateCalculator = coordinateCalculator ?? throw new ArgumentNullException(nameof(coordinateCalculator));
            _redactionProvider = redactionProvider;
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
                    implementationType: _redactionProvider.GetProviderType());

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

                telemetryEvent.PageCount = document.Pages.Count;
                telemetryEvent.OriginalNullCharCount = GetNullCharacterCount(document);

                AddAnnotations(document, redactPdfRequest, correlationId);

                Document sanitizedDocument;
                try
                {
                    sanitizedDocument = _redactionProvider.SanitizeDocument(document);
                }
                catch (Exception ex)
                {
                    _logger.LogMethodError(correlationId, nameof(RedactPdfAsync), "Could not sanitize document", ex);
                    sanitizedDocument = document;
                    telemetryEvent.IsSanitizeBroken = true;
                }
                telemetryEvent.SanitizedTime = DateTime.UtcNow;
                telemetryEvent.NullCharCount = GetNullCharacterCount(sanitizedDocument);

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

                    _redactionProvider.AttachAnnotation(annotationPage, annotationRectangle);
                }
            }
        }

        private int GetNullCharacterCount(Document document)
        {
            try
            {
                var textAbsorber = new TextAbsorber();
                textAbsorber.ExtractionOptions.FormattingMode = TextExtractionOptions.TextFormattingMode.Raw;
                document.Pages.Accept(textAbsorber);
                var extractedText = textAbsorber.Text;
                return extractedText.Count(c => c == 0);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private Stream SaveToStream(Document document)
        {
            var stream = new MemoryStream();
            document.Save(stream);
            return stream;
        }

        private string GetUploadFileName(string fileName)
        {
            var fileNameWithoutExtension = fileName.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) > -1 ? fileName.Split(".pdf", StringSplitOptions.RemoveEmptyEntries)[0] : fileName;
            return $"{fileNameWithoutExtension}_{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.pdf"; //restore save redaction to same storage for now, but with additional randomised identifier
        }
    }
}