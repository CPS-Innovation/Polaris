using System;
using System.IO;
using Aspose.Pdf;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService;

public class XpsPdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public XpsPdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
    }

    public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
    {
        var doc = _asposeItemFactory.CreateRenderedXpsPdfDocument(inputStream, correlationId);
        doc.Save(pdfStream, SaveFormat.Pdf);
        pdfStream.Seek(0, SeekOrigin.Begin);
    }
}
