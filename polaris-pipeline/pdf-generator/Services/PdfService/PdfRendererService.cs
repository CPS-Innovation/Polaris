using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Pdf;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Common.Constants;
using System.Linq;
using System.Configuration;

namespace pdf_generator.Services.PdfService;

public class PdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public PdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
    }

    public async Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, string documentId, Guid correlationId)
    {
        var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposePdf);
        var pdfStream = new MemoryStream();

        try
        {
            var doc = _asposeItemFactory.CreateRenderedPdfDocument(inputStream, correlationId);
            if (doc.IsEncrypted)
                throw new PdfEncryptionException();


            var linkAnnotations = doc.Pages.SelectMany(page => page.Annotations.OfType<Aspose.Pdf.Annotations.LinkAnnotation>());
            foreach (var annotation in linkAnnotations)
            {
                var uriAction = annotation.Action as Aspose.Pdf.Annotations.GoToURIAction;
                if (uriAction == null || string.IsNullOrEmpty(uriAction.URI)) continue;

                uriAction.URI = SetLinkUri(uriAction.URI);
            }

            await doc.SaveAsync(pdfStream, SaveFormat.Pdf, CancellationToken.None);
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
        }
        catch (IndexOutOfRangeException)
        {
            // Aspose.Pdf 24.2.0 throws IndexOutOfRangeException exception when converting
            // otherwise healthy PDFs
            conversionResult.RecordConversionQualifiedSuccess(inputStream);
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
        catch (Exception ex)
        {
            inputStream?.Dispose();
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

    private string SetLinkUri(string uri)
    {
        const string fileScheme = "file://";
        const string httpsScheme = "https:";
        
        if (uri.StartsWith(fileScheme, StringComparison.OrdinalIgnoreCase))
            return uri.Replace(fileScheme, $"{httpsScheme}//");

        if (uri.StartsWith("//"))
           return httpsScheme + uri;

        return uri;
    }


    public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
        throw new NotImplementedException();
    }
}
