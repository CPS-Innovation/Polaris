using System;
using System.IO;
using Aspose.Pdf;
using Common.Domain.Document;
using Common.Exceptions;
using Common.Extensions;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService;

public class PdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public PdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
    }

    public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
        var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposePdf);
        var pdfStream = new MemoryStream();

        try
        {
            var doc = _asposeItemFactory.CreateRenderedPdfDocument(inputStream, correlationId);
            if (doc.IsEncrypted)
                throw new PdfEncryptionException();
            
            doc.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        
            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (InvalidPasswordException ex)
        {
            inputStream?.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfPasswordProtected, ex.ToFormattedString());
        }
        catch (InvalidPdfFileFormatException ex)
        {
            inputStream?.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfInvalidFileFormat, ex.ToFormattedString());
        }
        catch (PdfException ex)
        {
            inputStream?.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfException, ex.ToFormattedString());
        }
        catch (PdfEncryptionException ex)
        {
            inputStream?.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.PdfEncrypted, ex.ToFormattedString());
        }
        
        return conversionResult;
    }
}
