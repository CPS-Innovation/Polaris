using Aspose.Pdf;

namespace pdf_generator.Services.DocumentRedaction.Aspose
{
  public interface IRedactionImplementation
  {
    (ProviderType, string) GetProviderType();

    void AttachAnnotation(Page page, Rectangle rect);

    void FinaliseAnnotations(ref Document doc);
  }
}

