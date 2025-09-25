using Aspose.Pdf;
using Common.Constants;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Exceptions;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class PdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public PdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory.ExceptionIfNull();
    }

    public async Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        var conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, PdfConverterType.AsposePdf);
        var pdfStream = new MemoryStream();

        try
        {
            var doc = _asposeItemFactory.CreateRenderedPdfDocument(readToPdfDto.Stream, readToPdfDto.CorrelationId);
            if (doc.IsEncrypted)
                throw new PdfEncryptionException();

            await doc.SaveAsync(pdfStream, SaveFormat.Pdf, CancellationToken.None);
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (IndexOutOfRangeException)
        {
            // Aspose.Pdf 24.2.0 throws IndexOutOfRangeException exception when converting
            // otherwise healthy PDFs
            conversionResult.RecordConversionQualifiedSuccess(readToPdfDto.Stream);
        }
        catch (InvalidPasswordException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfPasswordProtected, ex.ToFormattedString());
        }
        catch (InvalidPdfFileFormatException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfInvalidFileFormat, ex.ToFormattedString());
        }
        catch (PdfException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfException, ex.ToFormattedString());
        }
        catch (PdfEncryptionException ex)
        {
            readToPdfDto.Stream.Dispose();
            conversionResult.RecordConversionFailure(PdfConversionStatus.PdfEncrypted, ex.ToFormattedString());
        }
        catch (Exception ex)
        {
            readToPdfDto.Stream.Dispose();
            if (ex.Message.Contains("Permissions check failed"))
            {
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposePdfPasswordProtected, ex.ToFormattedString());
            }
            else
            {
                throw;
            }
        }

        return conversionResult;
    }
}
