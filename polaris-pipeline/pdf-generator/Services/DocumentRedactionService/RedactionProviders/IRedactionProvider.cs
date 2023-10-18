using Aspose.Pdf;

namespace pdf_generator.Services.DocumentRedactionService.RedactionProvider
{
    public interface IRedactionProvider
    {
        ProviderType GetProviderType();

        void AttachAnnotation(Page page, Rectangle rect);

        Document SanitizeDocument(Document document);
    }
}