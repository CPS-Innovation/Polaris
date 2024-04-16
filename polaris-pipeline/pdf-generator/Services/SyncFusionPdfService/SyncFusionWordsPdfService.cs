using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionWordsPdfService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionWordsPdfService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionDocIO);
      var pdfStream = new MemoryStream();

      try
      {
        var doc = _syncFusionItemFactory.CreateWordsDocument(inputStream, correlationId);
        DocIORenderer render = new DocIORenderer();
        render.Settings.ChartRenderingOptions.ImageFormat = (Syncfusion.OfficeChart.ExportImageFormat)ExportImageFormat.Jpeg;

        PdfDocument pdfDocument = render.ConvertToPDF(doc);

        pdfDocument.Save(pdfStream);

        render.Dispose();
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
