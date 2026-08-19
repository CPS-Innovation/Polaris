// <copyright file="AsposeRedactionProvider.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.DocumentRedaction.Aspose;

using global::Aspose.Pdf;
using global::Aspose.Pdf.Facades;
using Common.Dto.Request;
using Common.Telemetry;
using Microsoft.Extensions.Logging;
using pdf_redactor.TelemetryEvents;
using Common.Streaming;
using Common.Telemetry;
using Microsoft.Extensions.Logging;
using pdf_redactor.Functions;
using pdf_redactor.TelemetryEvents;

public class AsposeRedactionProvider(
    IRedactionImplementation redactionImplementation,
    ICoordinateCalculator coordinateCalculator,
    ILogger<AsposeRedactionProvider> logger) : IRedactionProvider
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
            var (providerType, providerDetails) = redactionImplementation.GetProviderType();
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
            };

            var document = new Document(inputStream);

            telemetryEvent.PdfFormat = document.PdfFormat.ToString();
            telemetryEvent.PageCount = document.Pages.Count;

            telemetryEvent.AddAnnotationsStartTime = DateTime.UtcNow;
            this.AddAnnotations(document, redactPdfRequest, providerType, correlationId);
            telemetryEvent.AddAnnotationsEndTime = DateTime.UtcNow;

            telemetryEvent.FinaliseAnnotationsStartTime = DateTime.UtcNow;
            this.FinaliseAnnotations(document, correlationId);
            telemetryEvent.FinaliseAnnotationsEndTime = DateTime.UtcNow;

            telemetryEvent.SanitiseStartTime = DateTime.UtcNow;
            SanitiseDocument(document);
            telemetryEvent.SanitiseEndTime = DateTime.UtcNow;

            var outputStream = new MemoryStream();
            await document.SaveAsync(outputStream, CancellationToken.None);
            outputStream.Position = 0;
            document.Dispose();

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

    private void AddAnnotations(Document document, RedactPdfRequestDto redactPdfRequest, ProviderType providerType, Guid correlationId)
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
                var translatedCoordinates = coordinateCalculator.CalculateRelativeCoordinates(
                    redactionPage.Width, redactionPage.Height, currentPage, boxToDraw, pdfInfo, correlationId);

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

                var annotationRectangle = new Rectangle(
                    x1,
                    y1,
                    x2,
                    y2);

                redactionImplementation.AttachAnnotation(annotationPage, annotationRectangle);
            }
        }
    }

    private void FinaliseAnnotations(Document document, Guid correlationId) =>
        redactionImplementation.FinaliseAnnotations(ref document, correlationId);
}
