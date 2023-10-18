using System.Collections.Generic;
using Aspose.Pdf;

namespace pdf_generator.Services.DocumentRedactionService.RedactionProvider
{
    public interface IRedactionProvider
    {
        (ProviderType, string) GetProviderDetails();

        void AttachAnnotation(Page page, Rectangle rect);

        void SanitizeDocument(ref Document document);
    }
}