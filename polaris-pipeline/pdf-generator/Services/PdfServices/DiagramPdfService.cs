using Aspose.Diagram;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class DiagramPdfService(IAsposeItemFactory asposeItemFactory) : IPdfService
{
    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeDiagrams);
        var pdfStream = new MemoryStream();

        var doc = asposeItemFactory.CreateDiagram(readToPdfDto.Stream, readToPdfDto.CorrelationId);
        doc.Save(pdfStream, SaveFileFormat.Pdf);
        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
        return Task.FromResult(conversionResult);
    }
}