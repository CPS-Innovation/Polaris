using Aspose.Cells;
using Common.Constants;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class CellsPdfService(IAsposeItemFactory asposeItemFactory) : IPdfService
{
    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeCells);
        var pdfStream = new MemoryStream();

        try
        {
            using var workbook = asposeItemFactory.CreateWorkbook(readToPdfDto.Stream, readToPdfDto.CorrelationId);
            workbook.Save(pdfStream, new PdfSaveOptions { OnePagePerSheet = true });
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (CellsException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeCellsGeneralError,
                ex.ToFormattedString());
        }

        return Task.FromResult(conversionResult);
    }
}