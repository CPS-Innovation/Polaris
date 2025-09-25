using Aspose.Email;
using Aspose.Words;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class EmailPdfService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public EmailPdfService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposeEmail);
        var mailMessageStream = new MemoryStream();
        var pdfStream = new MemoryStream();

        var mailMsg = _asposeItemFactory.CreateMailMessage(readToPdfDto.Stream, readToPdfDto.CorrelationId);
        mailMessageStream.Seek(0, SeekOrigin.Begin);
        mailMsg.Save(mailMessageStream, SaveOptions.DefaultMhtml);

        //// load the MTHML from memoryStream into a document
        var document = _asposeItemFactory.CreateMhtmlDocument(mailMessageStream, readToPdfDto.CorrelationId);
        document.Save(pdfStream, SaveFormat.Pdf);
        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
        return Task.FromResult(conversionResult);
    }
}