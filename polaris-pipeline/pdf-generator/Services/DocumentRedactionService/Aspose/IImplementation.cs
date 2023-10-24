using Aspose.Pdf;

namespace pdf_generator.Services.DocumentRedactionService.Aspose;

public interface IImplementation
{
  void AttachAnnotation(Page page, Rectangle rect);

  void FinaliseAnnotations(ref Document doc);
}