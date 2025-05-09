﻿using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Common.Dto.Request;
using Common.Telemetry;
using pdf_redactor.TelemetryEvents;
using Common.Streaming;
using pdf_redactor.Functions;

namespace pdf_redactor.Services.DocumentRedaction.Aspose;

public class AsposeRedactionProvider(
    IRedactionImplementation redactionImplementation,
    ICoordinateCalculator coordinateCalculator,
    ITelemetryClient telemetryClient) : IRedactionProvider
{

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
            telemetryEvent.OriginalNullCharCount = GetNullCharacterCount(document);

            telemetryEvent.AddAnnotationsStartTime = DateTime.UtcNow;
            AddAnnotations(document, redactPdfRequest, correlationId);
            telemetryEvent.AddAnnotationsEndTime = DateTime.UtcNow;

            telemetryEvent.FinaliseAnnotationsStartTime = DateTime.UtcNow;
            FinaliseAnnotations(document, correlationId);
            telemetryEvent.FinaliseAnnotationsEndTime = DateTime.UtcNow;

            telemetryEvent.SanitiseStartTime = DateTime.UtcNow;
            SanitiseDocument(document);
            telemetryEvent.SanitiseEndTime = DateTime.UtcNow;

            telemetryEvent.NullCharCount = GetNullCharacterCount(document);

            var outputStream = new MemoryStream();
            await document.SaveAsync(outputStream, CancellationToken.None);
            outputStream.Position = 0;
            document.Dispose();

            telemetryEvent.Bytes = outputStream.Length;
            telemetryEvent.EndTime = DateTime.UtcNow;
            telemetryClient.TrackEvent(telemetryEvent);

            return outputStream;
        }
        catch (Exception)
        {
            telemetryClient.TrackEventFailure(telemetryEvent);
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
                var translatedCoordinates = coordinateCalculator.CalculateRelativeCoordinates(redactionPage.Width,
                    redactionPage.Height, currentPage, boxToDraw, pdfInfo, correlationId);

                var annotationRectangle = new Rectangle(
                    translatedCoordinates.X1,
                    translatedCoordinates.Y1,
                    translatedCoordinates.X2,
                    translatedCoordinates.Y2);

                redactionImplementation.AttachAnnotation(annotationPage, annotationRectangle);
            }
        }
    }

    private void FinaliseAnnotations(Document document, Guid correlationId) =>
        redactionImplementation.FinaliseAnnotations(ref document, correlationId);

    private static int GetNullCharacterCount(Document document)
    {
        _ = document;
        // this is disabled as it has a performance time impact on redactions
        // we are temporarily returning -1 and recording this for telemetry 
        return -1;
        // try
        // {
        //     var textAbsorber = new TextAbsorber();
        //     textAbsorber.ExtractionOptions.FormattingMode = TextExtractionOptions.TextFormattingMode.Raw;
        //     document.Pages.Accept(textAbsorber);
        //     var extractedText = textAbsorber.Text;
        //     return extractedText.Count(c => c == 0);
        // }
        // catch (Exception)
        // {
        //     return -1;
        // }
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