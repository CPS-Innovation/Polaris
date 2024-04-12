
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;

namespace pdf_redactor.Services.DocumentRedaction.SyncFusion
{
  public interface IRedactionImplementation
  {
    void AttachAnnotation(PdfLoadedPage page, RectangleF rect);

    void FinaliseAnnotations(ref PdfLoadedDocument doc, Guid correlationId);
  }
}

