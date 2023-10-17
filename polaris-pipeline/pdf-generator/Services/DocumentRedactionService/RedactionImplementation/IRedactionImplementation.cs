using Aspose.Pdf;

namespace pdf_generator.Services.DocumentRedactionService.RedactionImplementation
{
    public interface IRedactionImplementation
    {
        ImplementationType GetImplementationType();

        void AttachAnnotation(Page page, Rectangle rect);

        Document SanitizeDocument(Document document);
    }
}