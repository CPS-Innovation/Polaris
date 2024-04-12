using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;

namespace pdf_redactor.Services.DocumentRedaction.SyncFusion.RedactionImplementations
{
  public class DirectImplementation : IRedactionImplementation
  {
    public void AttachAnnotation(PdfLoadedPage page, RectangleF rect)
    {
      var hardAnnotation = new PdfRedaction(rect, Color.Black);

      page.AddRedaction(hardAnnotation);
    }

    public void FinaliseAnnotations(ref PdfLoadedDocument doc, Guid correlationId) { }
  }
}