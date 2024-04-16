using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Syncfusion.Pdf;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionCellsPdfService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionCellsPdfService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionCell);
      var pdfStream = new MemoryStream();

      try
      {
        ExcelEngine excelEngine = new ExcelEngine();
        IApplication application = excelEngine.Excel;

        var doc = _syncFusionItemFactory.CreateWorkbookDocument(inputStream, application);
        XlsIORenderer render = new XlsIORenderer();

        PdfDocument pdfDocument = render.ConvertToPDF(doc);

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
