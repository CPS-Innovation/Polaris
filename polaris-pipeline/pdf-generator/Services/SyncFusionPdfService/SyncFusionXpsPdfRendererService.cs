using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.XlsIORenderer;
using Syncfusion.XPS;
using Syncfusion.Pdf;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionXpsPdfService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionXpsPdfService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionSlides);
      var pdfStream = new MemoryStream();

      try
      {

        XPSToPdfConverter converter = new XPSToPdfConverter();

        PdfDocument pdfDocument = converter.Convert(inputStream);

        pdfDocument.Save(pdfStream);

        pdfDocument.Close();
        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
      }
      catch (Exception)
      {
        inputStream?.Dispose();
      }

      return conversionResult;
    }
  }
}
