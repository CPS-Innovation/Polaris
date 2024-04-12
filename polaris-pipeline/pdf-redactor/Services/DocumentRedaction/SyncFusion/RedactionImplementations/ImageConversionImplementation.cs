using pdf_redactor.Domain;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Interactive;
using Syncfusion.PdfToImageConverter;
using Syncfusion.Pdf.Graphics;

namespace pdf_redactor.Services.DocumentRedaction.SyncFusion.RedactionImplementations
{
  public class ImageConversionImplementation : IRedactionImplementation
  {

    public ImageConversionImplementation()
    {
    }

    public void AttachAnnotation(PdfLoadedPage page, RectangleF rect)
    {
      var softAnnotation = new PdfRectangleAnnotation(rect, "")
      {
        Color = Color.Black,
        InnerColor = Color.Black,
        Flatten = true,
      };

      page.Annotations.Add(softAnnotation);
    }

    public void FinaliseAnnotations(ref PdfLoadedDocument document, Guid correlationId)
    {
      PdfToImageConverter imageConverter = new PdfToImageConverter();

      for (var pageNumber = 0; pageNumber < document.Pages.Count; pageNumber++)
      {
        using var stream = new MemoryStream();
        document.Save(stream);

        PdfPageBase page = document.Pages[pageNumber];

        if (page.Annotations.Count == 0)
        {
          continue;
        }

        var pageHeight = page.Size.Height;
        var pageWidth = page.Size.Width;

        imageConverter.Load(stream);

        var bitmap = imageConverter.Convert(pageNumber, false, false);

        document.Pages.RemoveAt(pageNumber);

        var pageToSwapIn = document.Pages.Insert(pageNumber, new SizeF(pageWidth, pageHeight), new PdfMargins()
        {
          Top = 0,
          Bottom = 0,
          Left = 0,
          Right = 0

        });
        pageToSwapIn.Graphics.DrawImage(PdfImage.FromStream(bitmap), new PointF(0, 0), new SizeF(pageWidth, pageHeight));

      }
    }
  }
}