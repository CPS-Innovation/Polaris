using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.Pdf;
using Syncfusion.PresentationRenderer;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionSlidesPdfService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionSlidesPdfService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionSlides);
      var pdfStream = new MemoryStream();

      try
      {

        var doc = _syncFusionItemFactory.CreateSlidesDocument(inputStream);

        PdfDocument pdfDocument = PresentationToPdfConverter.Convert(doc);

        pdfDocument.Save(pdfStream);

        pdfDocument.Close();
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
