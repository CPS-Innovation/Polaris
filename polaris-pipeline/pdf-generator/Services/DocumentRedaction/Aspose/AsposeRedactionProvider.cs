using System;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Common.Dto.Request;
using Common.Telemetry.Contracts;
using pdf_generator.TelemetryEvents;
using Aspose.Pdf.Text;
using System.Linq;
using Common.Streaming;
using System.Threading.Tasks;

namespace pdf_generator.Services.DocumentRedaction.Aspose
{
    public class AsposeRedactionProvider : IRedactionProvider
    {
        private readonly IRedactionImplementation _redactionImplementation;
        private readonly ICoordinateCalculator _coordinateCalculator;
        private readonly ITelemetryClient _telemetryClient;

        public AsposeRedactionProvider(
            IRedactionImplementation implementation,
            ICoordinateCalculator coordinateCalculator,
            ITelemetryClient telemetryClient)
        {
            _redactionImplementation = implementation;
            _coordinateCalculator = coordinateCalculator ?? throw new ArgumentNullException(nameof(coordinateCalculator));
            _telemetryClient = telemetryClient;
        }

        public async Task<Stream> Redact(Stream stream, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            RedactedDocumentEvent telemetryEvent = default;
            try
            {
                var inputStream = await stream.EnsureSeekableAsync();
                var (providerType, providerDetails) = _redactionImplementation.GetProviderType();
                telemetryEvent = new RedactedDocumentEvent(correlationId: correlationId,
                    caseId: caseId,
                    documentId: documentId,
                    redactionPageCounts: redactPdfRequest.RedactionPageCounts(),
                    providerType: providerType,
                    providerDetails: providerDetails,
                    startTime: DateTime.UtcNow,
                    originalBytes: inputStream.Length);

                var document = new Document(inputStream);

                telemetryEvent.PdfFormat = document.PdfFormat.ToString();
                telemetryEvent.PageCount = document.Pages.Count;
                telemetryEvent.OriginalNullCharCount = GetNullCharacterCount(document);

                AddAnnotations(document, redactPdfRequest, correlationId);
                FinaliseAnnotations(document);
                SanitiseDocument(document);

                telemetryEvent.NullCharCount = GetNullCharacterCount(document);

                var outputStream = new MemoryStream();
                document.Save(outputStream);
                outputStream.Position = 0;
                document.Dispose();

                telemetryEvent.Bytes = outputStream.Length;
                telemetryEvent.EndTime = DateTime.UtcNow;
                _telemetryClient.TrackEvent(telemetryEvent);

                return outputStream;
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

        private void FinaliseAnnotations(Document document)
        {
            _redactionImplementation.FinaliseAnnotations(ref document);
        }

        private static int GetNullCharacterCount(Document document)
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
        public static void SanitiseDocument(Document document)
        {
            document.RemoveMetadata();

            if (IsCandidateForConversion(document))
            {
                document.Convert(
                    // `Convert` streams feedback here, we are not interested currently
                    //  Note: if we let Aspose do its default behaviour it tries to write
                    //  to a ConversionLog.xml file, which blows up in production as our azure function
                    //  is set as read-only due to our packaged deployment.
                    new MemoryStream(),
                    PdfFormat.v_1_7,
                    ConvertErrorAction.Delete);
            }
        }

        private static bool IsCandidateForConversion(Document document)
        {

            return (document.PdfFormat is PdfFormat.v_1_0
                        or PdfFormat.v_1_1
                        or PdfFormat.v_1_2
                        or PdfFormat.v_1_3
                        or PdfFormat.v_1_4
                        or PdfFormat.v_1_5
                        or PdfFormat.v_1_6)
                    && document.Validate(
                        // `Validate` streams feedback here, we are not interested currently
                        //  Note: if we let Aspose do its default behaviour it tries to write
                        //  to a ConversionLog.xml file, which blows up in production as our azure function
                        //  is set as read-only due to our packaged deployment.
                        new MemoryStream(),
                        PdfFormat.v_1_7);
        }
    }
}