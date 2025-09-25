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

public class WordsPdfService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public WordsPdfService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public virtual Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeWords);
        var pdfStream = new MemoryStream();

        try
        {
            var doc = new Document(readToPdfDto.Stream);
            doc.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (UnsupportedFileFormatException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeWordsUnsupportedFileFormat,
                ex.ToFormattedString());
        }
        catch (IncorrectPasswordException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeWordsPasswordProtected,
                ex.ToFormattedString());
        }

        return Task.FromResult(conversionResult);
    }
}