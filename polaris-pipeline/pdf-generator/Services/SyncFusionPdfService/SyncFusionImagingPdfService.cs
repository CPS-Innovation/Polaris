using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.Pdf;
using Syncfusion.PresentationRenderer;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionImagingPdfService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionImagingPdfService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionImaging);
      var pdfStream = new MemoryStream();

      try
      {

        var doc = new PdfDocument();
        var page = doc.Pages.Add();
        var graphics = page.Graphics;

        var image = _syncFusionItemFactory.CreateImageDocument(inputStream);

        graphics.DrawImage(image, 0, 0);

        doc.Save(pdfStream);

        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
      }
      catch (Exception ex)
      {
        conversionResult.RecordConversionFailure(PdfConversionStatus.UnexpectedError, ex.ToFormattedString());
        inputStream?.Dispose();
      }

      return conversionResult;
    }
  }
}
