// <copyright file="AsposeRedactionProvider.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.DocumentRedaction.Aspose;

using Common.Dto.Request;
using Common.Streaming;
using Common.Telemetry;
using global::Aspose.Pdf;
using global::Aspose.Pdf.Facades;
using global::Aspose.Pdf.Text;
using Microsoft.Extensions.Logging;
using pdf_redactor.Functions;
using pdf_redactor.TelemetryEvents;

public class AsposeRedactionProvider(
    IEnumerable<IRedactionImplementation> redactionImplementations,
    ICoordinateCalculator coordinateCalculator,
    ILogger<AsposeRedactionProvider> logger)
    : IRedactionProvider
{
    private const double DirectRedactionInsetPoints = 0.5;

    public static void SanitiseDocument(Document document)
    {
        document.RemoveMetadata();

        if (IsCandidateForConversion(document))
        {
            /*`Convert` streams feedback here, we are not interested currently
                Note: if we let Aspose do its default behaviour it tries to write
                to a ConversionLog.xml file, which blows up in production as our azure function
                is set as read-only due to our packaged deployment. */
            _ = document.Convert(
                new MemoryStream(),
                PdfFormat.v_1_7,
                ConvertErrorAction.Delete);
        }
    }

    public async Task<Stream> Redact(Stream stream, int caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
    {
        RedactedDocumentEvent? telemetryEvent = default;
        try
        {
            var inputStream = await stream.EnsureSeekableAsync();
            inputStream.Position = 0;

            using var document = new Document(inputStream);

            var (redactionImplementation, providerType, providerDetails) = this.ResolveImplementation(document, redactPdfRequest);

            telemetryEvent = new RedactedDocumentEvent(
                correlationId,
                caseId,
                documentId,
                redactPdfRequest.RedactionPageCounts(),
                providerType,
                providerDetails,
                DateTime.UtcNow,
                inputStream.Length)
            {
                OperationName = nameof(RedactPdf),
                PdfFormat = document.PdfFormat.ToString(),
                PageCount = document.Pages.Count,
                AddAnnotationsStartTime = DateTime.UtcNow,
            };

            this.AddAnnotations(document, redactPdfRequest, providerType, correlationId, redactionImplementation);
            telemetryEvent.AddAnnotationsEndTime = DateTime.UtcNow;

            telemetryEvent.FinaliseAnnotationsStartTime = DateTime.UtcNow;
            FinaliseAnnotations(document, correlationId, redactionImplementation);
            telemetryEvent.FinaliseAnnotationsEndTime = DateTime.UtcNow;

            telemetryEvent.SanitiseStartTime = DateTime.UtcNow;
            SanitiseDocument(document);
            telemetryEvent.SanitiseEndTime = DateTime.UtcNow;

            var outputStream = new MemoryStream();
            await document.SaveAsync(outputStream, CancellationToken.None);
            outputStream.Position = 0;

            telemetryEvent.Bytes = outputStream.Length;
            telemetryEvent.EndTime = DateTime.UtcNow;
            logger.TrackEvent(telemetryEvent);

            return outputStream;
        }
        catch (Exception)
        {
            logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }

    private static bool IsCandidateForConversion(Document document)
    {
        // `Validate` streams feedback here, we are not interested currently
        //  Note: if we let Aspose do its default behaviour it tries to write
        //  to a ConversionLog.xml file, which blows up in production as our azure function
        //  is set as read-only due to our packaged deployment.
        return (document.PdfFormat is PdfFormat.v_1_0
                    or PdfFormat.v_1_1
                    or PdfFormat.v_1_2
                    or PdfFormat.v_1_3
                    or PdfFormat.v_1_4
                    or PdfFormat.v_1_5
                    or PdfFormat.v_1_6)
               && document.Validate(
                   new MemoryStream(),
                   PdfFormat.v_1_7);
    }

    private static void FinaliseAnnotations(Document document, Guid correlationId, IRedactionImplementation redactionImplementation) =>
        redactionImplementation.FinaliseAnnotations(ref document, correlationId);

    private static bool HasValidTextLayer(
        Document document,
        RedactPdfRequestDto redactPdfRequest,
        out string? failureReason)
    {
        failureReason = null;

        if (document.Pages.Count == 0)
        {
            failureReason = "The PDF does not contain any pages.";
            return false;
        }

        var annotatedPageNumbers = redactPdfRequest
            .RedactionDefinitions
            .Select(redactionDefinition => redactionDefinition.PageIndex)
            .Distinct()
            .ToList();

        if (annotatedPageNumbers.Count == 0)
        {
            return true;
        }

        foreach (var pageNumber in annotatedPageNumbers)
        {
            if (pageNumber < 1 || pageNumber > document.Pages.Count)
            {
                failureReason = $"Annotation page {pageNumber} is outside the PDF page range of 1 to {document.Pages.Count}.";

                return false;
            }

            var page = document.Pages[pageNumber];

            var searchOptions = new TextSearchOptions(false)
            {
                /*
                 * Report errors caused by missing or invalid character
                 * mappings instead of silently accepting the extracted text.
                 */
                LogTextExtractionErrors = true,

                /*
                 * A missing or invalid font resource makes direct text
                 * redaction unsafe.
                 */
                IgnoreResourceFontErrors = false,

                /*
                 * Annotation text is not part of the underlying page text
                 * layer that will be redacted.
                 */
                SearchInAnnotations = false,

                /*
                 * Text-related graphics are not required for validating
                 * whether the page has extractable text.
                 */
                SearchForTextRelatedGraphics = false,
            };

            var extractionOptions = new TextExtractionOptions(
                TextExtractionOptions.TextFormattingMode.Pure);

            var textAbsorber = new TextAbsorber(
                extractionOptions,
                searchOptions);

            try
            {
                page.Accept(textAbsorber);
            }
            catch (Exception exception)
            {
                failureReason = $"Text extraction failed on annotation page {pageNumber}. {exception.GetType().Name}: {exception.Message}";

                return false;
            }

            if (textAbsorber.HasErrors)
            {
                failureReason = $"Aspose reported {textAbsorber.Errors.Count} text extraction or decoding error(s) on annotation page {pageNumber}.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(textAbsorber.Text))
            {
                failureReason = $"No extractable text was found on annotation page {pageNumber}.";

                return false;
            }
        }

        return true;
    }

    private (IRedactionImplementation Implementation, ProviderType ProviderType, string? ProviderDetails) ResolveImplementation(Document document, RedactPdfRequestDto redactPdfRequest)
    {
        var hasValidTextLayer = HasValidTextLayer(document, redactPdfRequest, out var failureReason);

        var selectedProviderType = hasValidTextLayer
            ? ProviderType.DirectRedaction
            : ProviderType.ImageConversion;

        var selectedImplementation = redactionImplementations
            .SingleOrDefault(
                implementation =>
                    implementation.GetProviderType().Item1 ==
                    selectedProviderType)
            ?? throw new InvalidOperationException(
                $"No redaction implementation registered for provider " +
                $"type '{selectedProviderType}'.");

        var providerDetails = hasValidTextLayer
            ? null
            : $"Fallback to ImageConversion. Reason: {failureReason}";

        return (selectedImplementation, selectedProviderType, providerDetails);
    }

    private void AddAnnotations(
        Document document,
        RedactPdfRequestDto redactPdfRequest,
        ProviderType providerType,
        Guid correlationId,
        IRedactionImplementation redactionImplementation)
    {
        var pdfInfo = new PdfFileInfo(document);
        var shouldInsetDirectRedaction = providerType == ProviderType.DirectRedaction;

        foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
        {
            var currentPage = redactionPage.PageIndex;
            var annotationPage = document.Pages[currentPage];
            var pageRect = annotationPage.GetPageRect(true);

            foreach (var boxToDraw in redactionPage.RedactionCoordinates)
            {
                var translatedCoordinates = coordinateCalculator.CalculateRelativeCoordinates(redactionPage.Width, redactionPage.Height, currentPage, boxToDraw, pdfInfo, correlationId);

                var x1 = Math.Min(translatedCoordinates.X1, translatedCoordinates.X2);
                var x2 = Math.Max(translatedCoordinates.X1, translatedCoordinates.X2);
                var y1 = Math.Min(translatedCoordinates.Y1, translatedCoordinates.Y2);
                var y2 = Math.Max(translatedCoordinates.Y1, translatedCoordinates.Y2);

                // Clamp annotation bounds to the page to avoid over-redaction side effects.
                x1 = Math.Clamp(x1, pageRect.LLX, pageRect.URX);
                x2 = Math.Clamp(x2, pageRect.LLX, pageRect.URX);
                y1 = Math.Clamp(y1, pageRect.LLY, pageRect.URY);
                y2 = Math.Clamp(y2, pageRect.LLY, pageRect.URY);

                if (shouldInsetDirectRedaction)
                {
                    // Keep a tiny buffer from adjacent glyph bounds for direct redactions.
                    x1 += DirectRedactionInsetPoints;
                    x2 -= DirectRedactionInsetPoints;
                    y1 += DirectRedactionInsetPoints;
                    y2 -= DirectRedactionInsetPoints;
                }

                if (x2 <= x1 || y2 <= y1)
                {
                    continue;
                }

                var annotationRectangle = new Rectangle(x1, y1, x2, y2);

                redactionImplementation.AttachAnnotation(annotationPage, annotationRectangle);
            }
        }
    }
}
