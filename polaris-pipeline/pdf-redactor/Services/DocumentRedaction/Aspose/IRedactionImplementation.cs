// <copyright file="IRedactionImplementation.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.DocumentRedaction.Aspose;

using global::Aspose.Pdf;

public interface IRedactionImplementation
{
    (ProviderType, string?) GetProviderType();

    void AttachAnnotation(Page page, Rectangle rect);

    void FinaliseAnnotations(ref Document doc, Guid correlationId);
}
