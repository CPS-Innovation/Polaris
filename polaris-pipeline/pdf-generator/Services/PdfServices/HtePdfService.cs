using Aspose.Words;
using Common.Constants;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class HtePdfService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public HtePdfService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeHte);
        var pdfStream = new MemoryStream();

        try
        {
            var doc = _asposeItemFactory.CreateHtmlDocument(readToPdfDto.Stream, readToPdfDto.CorrelationId);
            doc.Save(pdfStream);
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (UnsupportedFileFormatException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeHtmlUnsupportedFileFormat,
                ex.ToFormattedString());
        }
        catch (IncorrectPasswordException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeHtmlPasswordProtected,
                ex.ToFormattedString());
        }

        return Task.FromResult(conversionResult);
    }
}