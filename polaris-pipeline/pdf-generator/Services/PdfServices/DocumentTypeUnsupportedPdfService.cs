using Common.Constants;
using pdf_generator.Domain.Document;
using pdf_generator.Models;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class DocumentTypeUnsupportedPdfService : IPdfService
{
    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.None);
        conversionResult.RecordConversionFailure(PdfConversionStatus.DocumentTypeUnsupported, "File type is currently unsupported by Polaris");
        return Task.FromResult(conversionResult);
    }
}