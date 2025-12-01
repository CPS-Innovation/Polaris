using Aspose.Pdf;
using Common.Constants;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            MergeLinkAnnotations(doc);

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

    private void MergeLinkAnnotations(Aspose.Pdf.Document doc)
    {
        const double mergeThreshold = 50.0;

        var grouped = doc.Pages
            .SelectMany(page => page.Annotations.OfType<Aspose.Pdf.Annotations.LinkAnnotation>()
                .Select(a => new { Annotation = a, Page = page }))
            .Where(x => x.Annotation.Action is Aspose.Pdf.Annotations.GoToURIAction)
            .GroupBy(x => new { Uri = ((Aspose.Pdf.Annotations.GoToURIAction)x.Annotation.Action).URI, Page = x.Page });

        foreach (var group in grouped)
        {
            var uri = SetLinkUri(group.Key.Uri);

            // Cluster annotations by proximity
            var clusters = group.OrderBy(x => x.Annotation.Rect.LLY)
                .Aggregate(new List<List<(Aspose.Pdf.Annotations.LinkAnnotation Annotation, Aspose.Pdf.Page Page)>>(),
                    (acc, current) =>
                    {
                        if (!acc.Any() || Math.Abs(current.Annotation.Rect.LLY - acc.Last().Last().Annotation.Rect.LLY) > mergeThreshold)
                            acc.Add(new List<(Aspose.Pdf.Annotations.LinkAnnotation, Aspose.Pdf.Page)>());

                        acc.Last().Add((current.Annotation, current.Page));
                        return acc;
                    });

            // Merge clusters
            clusters.ForEach(cluster =>
            {
                var rects = cluster.Select(c => c.Annotation.Rect);
                var combinedRect = new Aspose.Pdf.Rectangle(rects.Min(r => r.LLX), rects.Min(r => r.LLY), rects.Max(r => r.URX), rects.Max(r => r.URY));

                cluster.ForEach(c => c.Page.Annotations.Delete(c.Annotation));
                cluster.First().Page.Annotations.Add(new Aspose.Pdf.Annotations.LinkAnnotation(cluster.First().Page, combinedRect)
                {
                    Action = new Aspose.Pdf.Annotations.GoToURIAction(uri)
                });
            });
        }
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
