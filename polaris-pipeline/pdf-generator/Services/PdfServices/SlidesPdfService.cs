using Aspose.Slides;
using Aspose.Slides.Export;
using Common.Constants;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class SlidesPdfService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public SlidesPdfService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeSlides);
        var pdfStream = new MemoryStream();

        try
        {
            using var presentation = _asposeItemFactory.CreatePresentation(readToPdfDto.Stream, readToPdfDto.CorrelationId);
            presentation.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (InvalidPasswordException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeSlidesPasswordProtected, ex.ToFormattedString());
        }

        return Task.FromResult(conversionResult);
    }
}