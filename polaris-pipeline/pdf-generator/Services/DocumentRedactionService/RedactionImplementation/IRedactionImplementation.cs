using Aspose.Pdf;

namespace pdf_generator.Services.DocumentRedactionService.RedactionImplementation
{
    public interface IRedactionImplementation
    {
        void AttachAnnotation(Page page, Rectangle rect);

        Document SanitizeDocument(Document document);
    }
}