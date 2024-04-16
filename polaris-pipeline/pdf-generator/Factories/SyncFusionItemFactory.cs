using System;
using System.IO;
using pdf_generator.Factories.Contracts;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.Presentation;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

namespace pdf_generator.Factories
{
  public class SyncFusionItemFactory : ISyncFusionItemFactory
  {
    public WordDocument CreateWordsDocument(Stream inputStream, Guid correlationId) => new(inputStream, Syncfusion.DocIO.FormatType.Automatic);

    public IWorkbook CreateWorkbookDocument(Stream inputStream, IApplication application) => application.Workbooks.Open(inputStream);
    public IPresentation CreateSlidesDocument(Stream inputStream) => Presentation.Open(inputStream);
    public PdfBitmap CreateImageDocument(Stream inputStream) => new(inputStream);
    public PdfLoadedDocument CreateRendererdPdfDocument(Stream inputStream) => new PdfLoadedDocument(inputStream);

  }
}