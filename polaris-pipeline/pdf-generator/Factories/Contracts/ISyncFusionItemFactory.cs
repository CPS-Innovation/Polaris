using System;
using System.IO;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.Presentation;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

namespace pdf_generator.Factories.Contracts
{
  public interface ISyncFusionItemFactory
  {

    public WordDocument CreateWordsDocument(Stream inputStream, Guid correlationId);
    public IWorkbook CreateWorkbookDocument(Stream inputStream, IApplication application);
    public IPresentation CreateSlidesDocument(Stream inputStream);
    public PdfBitmap CreateImageDocument(Stream inputStream);
    public PdfLoadedDocument CreateRendererdPdfDocument(Stream inputStream);

  }
}

