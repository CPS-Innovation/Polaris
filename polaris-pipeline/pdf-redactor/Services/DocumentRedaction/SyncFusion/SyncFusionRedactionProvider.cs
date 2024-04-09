using System;
using System.IO;
using Common.Dto.Request;
using Common.Telemetry;
using System.Linq;
using Common.Streaming;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;
using Syncfusion.Drawing;
using Common.Dto.Request.Redaction;

namespace pdf_redactor.Services.DocumentRedaction.SyncFusion
{
  public class SyncFusionRedactionProvider : IRedactionProvider
  {

    public SyncFusionRedactionProvider()
    {
    }

    public async Task<Stream> Redact(Stream stream, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId, RedactionType redactionType)
    {
      try
      {
        var inputStream = await stream.EnsureSeekableAsync();

        var document = new PdfLoadedDocument(inputStream);

        AddAnnotations(document, redactPdfRequest, correlationId);
        document.Redact();

        var outputStream = new MemoryStream();
        document.Save(outputStream);
        outputStream.Position = 0;
        document.Dispose();

        return outputStream;
      }
      catch (Exception)
      {
        throw;
      }
    }

    private void AddAnnotations(PdfLoadedDocument document, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
    {

      foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
      {
        var currentPage = redactionPage.PageIndex - 1;
        var annotationPage = document.Pages[currentPage] as PdfLoadedPage;


        foreach (var boxToDraw in redactionPage.RedactionCoordinates)
        {
          var relativeCoords = CalculateRelativeCoordinates(redactionPage.Width, redactionPage.Height, boxToDraw, annotationPage);

          float documentHeight = annotationPage.Size.Height;

          RectangleF rectangle = new RectangleF(new PointF((float)relativeCoords.X1, documentHeight - (float)relativeCoords.Y1),
                                                new SizeF((float)(relativeCoords.X2 - relativeCoords.X1),
                                                          (float)(relativeCoords.Y1 - relativeCoords.Y2)));

          annotationPage.AddRedaction(new PdfRedaction(rectangle, Color.Black));
        }
      }
    }

    public static RedactionCoordinatesDto CalculateRelativeCoordinates(double pageWidth, double pageHeight, RedactionCoordinatesDto originatorCoordinates, PdfLoadedPage page)
    {

      var pdfTranslatedCoordinates = new RedactionCoordinatesDto();
      var x1Cent = originatorCoordinates.X1 * 100 / pageWidth;
      var y1Cent = originatorCoordinates.Y1 * 100 / pageHeight;
      var x2Cent = originatorCoordinates.X2 * 100 / pageWidth;
      var y2Cent = originatorCoordinates.Y2 * 100 / pageHeight;

      var pdfWidth = page.Size.Width;
      var pdfHeight = page.Size.Height;

      pdfTranslatedCoordinates.X1 = pdfWidth / 100 * x1Cent;
      pdfTranslatedCoordinates.Y1 = pdfHeight / 100 * y1Cent;
      pdfTranslatedCoordinates.X2 = pdfWidth / 100 * x2Cent;
      pdfTranslatedCoordinates.Y2 = pdfHeight / 100 * y2Cent;

      return pdfTranslatedCoordinates;
    }
  }
}