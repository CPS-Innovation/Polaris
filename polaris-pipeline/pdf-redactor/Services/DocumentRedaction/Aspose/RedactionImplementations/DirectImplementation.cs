using Aspose.Pdf;
using Aspose.Pdf.Annotations;

namespace pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations
{
    public class DirectImplementation : IRedactionImplementation
    {
        public void AttachAnnotation(Page page, Rectangle rect)
        {
            var hardAnnotation = new RedactionAnnotation(page, rect)
            {
                FillColor = Color.Black,
            };

            page.Annotations.Add(hardAnnotation, true);
            hardAnnotation.Redact();
        }

        public void FinaliseAnnotations(ref Document doc) { }

        public (ProviderType, string) GetProviderType()
        {
            return (ProviderType.DirectRedaction, null);
        }
    }
}