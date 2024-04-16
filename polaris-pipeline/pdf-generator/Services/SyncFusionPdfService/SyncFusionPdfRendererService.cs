using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.Pdf;
using Syncfusion.PresentationRenderer;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionPdfRendererService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionPdfRendererService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionPdf);
      var pdfStream = new MemoryStream();

      try
      {

        var doc = _syncFusionItemFactory.CreateRendererdPdfDocument(inputStream);

        if (doc.IsEncrypted)
          throw new PdfEncryptionException();


        doc.Save(pdfStream);

        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
      }
      catch (PdfEncryptionException ex)
      {
        inputStream?.Dispose();
        conversionResult.RecordConversionFailure(PdfConversionStatus.PdfEncrypted, ex.ToFormattedString());
      }
      catch (Exception ex)
      {
        inputStream?.Dispose();
        conversionResult.RecordConversionFailure(PdfConversionStatus.UnexpectedError, ex.ToFormattedString());
      }

      return conversionResult;
    }
  }
}
