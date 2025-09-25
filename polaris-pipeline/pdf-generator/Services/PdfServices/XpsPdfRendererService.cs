using Aspose.Pdf;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class XpsPdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public XpsPdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeCells);
        var pdfStream = new MemoryStream();

        var doc = _asposeItemFactory.CreateRenderedXpsPdfDocument(readToPdfDto.Stream, readToPdfDto.CorrelationId);
        doc.Save(pdfStream, SaveFormat.Pdf);
        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
        return Task.FromResult(conversionResult);
    }
}
