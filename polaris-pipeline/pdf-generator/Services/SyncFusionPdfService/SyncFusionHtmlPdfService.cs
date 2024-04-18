using System;
using System.IO;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace pdf_generator.Services.PdfService
{
  public class SyncFusionHtmlPdfService : ISyncFusionPdfService
  {
    private readonly ISyncFusionItemFactory _syncFusionItemFactory;

    public SyncFusionHtmlPdfService(ISyncFusionItemFactory syncFusionItemFactory)
    {
      _syncFusionItemFactory = syncFusionItemFactory;
    }

    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
    {
      var conversionResult = new SyncFusionPdfConversionResult(documentId, SyncFusionPdfConverterType.SyncFusionSlides);
      var pdfStream = new MemoryStream();

      try
      {
        var reader = new StreamReader(inputStream);
        string htmlString = reader.ReadToEnd();

        HtmlToPdfConverter converter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
        BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
        //Set Blink viewport size.
        blinkConverterSettings.ViewPortSize = new Syncfusion.Drawing.Size(1280, 0);
        //Assign Blink converter settings to HTML converter.
        converter.ConverterSettings = blinkConverterSettings;

        PdfDocument pdfDocument = converter.Convert(htmlString, string.Empty);

        pdfDocument.Save(pdfStream);

        pdfDocument.Close();
        pdfStream.Seek(0, SeekOrigin.Begin);

        conversionResult.RecordConversionSuccess(pdfStream);
      }
      catch (Exception ex)
      {
        conversionResult.RecordConversionFailure(PdfConversionStatus.UnexpectedError, ex.ToFormattedString());
        inputStream?.Dispose();
      }

      return conversionResult;
    }
  }
}
