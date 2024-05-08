using System;
using System.IO;
using Aspose.Pdf;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService;

public class XpsPdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public XpsPdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
    }

    public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
        var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeCells);
        var pdfStream = new MemoryStream();
        
        var doc = _asposeItemFactory.CreateRenderedXpsPdfDocument(inputStream, correlationId);
        doc.Save(pdfStream, SaveFormat.Pdf);
        pdfStream.Seek(0, SeekOrigin.Begin);
        
        conversionResult.RecordConversionSuccess(pdfStream);
        return conversionResult;
    }
}
