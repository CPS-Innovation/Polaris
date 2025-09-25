using Aspose.Imaging.CoreExceptions;
using Aspose.Imaging.FileFormats.Pdf;
using Aspose.Imaging.ImageOptions;
using Common.Constants;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class ImagingPdfService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public ImagingPdfService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeImaging);
        var pdfStream = new MemoryStream();

        try
        {
            using var image = _asposeItemFactory.CreateImage(readToPdfDto.Stream, readToPdfDto.CorrelationId);
            image.Save(pdfStream, new PdfOptions { PdfDocumentInfo = new PdfDocumentInfo() });
            pdfStream.Seek(0, System.IO.SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (ImageLoadException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeImagingCannotLoad,
                ex.ToFormattedString());
        }

        return Task.FromResult(conversionResult);
    }
}