// <copyright file="DirectImplementation.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

using global::Aspose.Pdf;
using global::Aspose.Pdf.Annotations;

public class DirectImplementation : IRedactionImplementation
{
    public void AttachAnnotation(Page page, Rectangle rect)
    {
        var redaction = new RedactionAnnotation(page, rect)
        {
            FillColor = Color.Black,
            Color = Color.Black,
            OverlayText = null,
            Opacity = 1,
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
