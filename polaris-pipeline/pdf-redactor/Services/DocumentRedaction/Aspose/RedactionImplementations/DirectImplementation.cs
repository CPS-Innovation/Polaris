using Aspose.Pdf;
using Aspose.Pdf.Annotations;

namespace pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

public class DirectImplementation : IRedactionImplementation
{
    public void AttachAnnotation(Page page, Rectangle rect)
    {
        var redaction = new RedactionAnnotation(page, rect)
        {
            FillColor = Color.Black,
            Color = Color.Black,
        };

        page.Annotations.Add(redaction, true);
    }

    public void FinaliseAnnotations(ref Document doc, Guid correlationId)
    {
        foreach (Page page in doc.Pages)
        {
            var redactions = page.Annotations
                .Where(a => a.AnnotationType == AnnotationType.Redaction)
                .Cast<RedactionAnnotation>()
                .ToList();

            foreach (var redaction in redactions)
            {
                redaction.Redact();
            }
        }
    }

    public (ProviderType, string?) GetProviderType()
    {
        return (ProviderType.DirectRedaction, null);
    }
}
