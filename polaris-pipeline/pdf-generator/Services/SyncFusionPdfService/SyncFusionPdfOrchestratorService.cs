
using System;
using System.IO;
using pdf_generator.Services.PdfService;
using pdf_generator.Domain.Document;
using Common.Domain.Document;
using pdf_generator.Extensions;

namespace pdf_generator.Services.SyncFusionPdfService
{
  public class SyncFusionPdfOrchestratorService : ISyncFusionPdfOrchestratorService
  {
    private readonly ISyncFusionPdfService _wordsPdfService;
    private readonly ISyncFusionPdfService _htmlPdfService;
    private readonly ISyncFusionPdfService _cellsPdfService;
    private readonly ISyncFusionPdfService _slidesPdfService;
    private readonly ISyncFusionPdfService _imagingPdfService;
    private readonly ISyncFusionPdfService _xpsPdfRendererService;
    private readonly ISyncFusionPdfService _pdfRendererService;

    public SyncFusionPdfOrchestratorService(
      ISyncFusionPdfService wordsPdfService,
      ISyncFusionPdfService cellsPdfService,
      ISyncFusionPdfService slidesPdfService,
      ISyncFusionPdfService imagingPdfService,
      ISyncFusionPdfService xpsPdfRendererService,
      ISyncFusionPdfService htmlPdfService,
      ISyncFusionPdfService pdfRendererService
      )
    {
      _wordsPdfService = wordsPdfService;
      _cellsPdfService = cellsPdfService;
      _slidesPdfService = slidesPdfService;
      _imagingPdfService = imagingPdfService;
      _xpsPdfRendererService = xpsPdfRendererService;
      _htmlPdfService = htmlPdfService;
      _pdfRendererService = pdfRendererService;
    }
    public SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, FileType fileType, string documentId, Guid correlationId)
    {
      SyncFusionPdfConversionResult conversionResult;
      var converterType = SyncFusionPdfConverterType.None;

      try
      {
        switch (fileType)
        {
          case FileType.DOC:
          case FileType.DOCX:
          case FileType.DOCM:
          case FileType.DOT:
          case FileType.DOTX:
          case FileType.DOTM:
          case FileType.RTF:
          case FileType.TXT:
            converterType = SyncFusionPdfConverterType.SyncFusionDocIO;
            conversionResult = _wordsPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;

          case FileType.HTML:
          case FileType.HTM:
          case FileType.MHT:
          case FileType.MHTML:
          case FileType.HTE:
          case FileType.EML:
          case FileType.MSG:
            converterType = SyncFusionPdfConverterType.SyncFusionHtml;
            conversionResult = _htmlPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;

          case FileType.CSV:
          case FileType.XLS:
          case FileType.XLSX:
          case FileType.XLSM:
          case FileType.XLT:
            converterType = SyncFusionPdfConverterType.SyncFusionCell;
            conversionResult = _cellsPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;

          case FileType.PPTX:
            converterType = SyncFusionPdfConverterType.SyncFusionSlides;
            conversionResult = _slidesPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;

          case FileType.BMP:
          case FileType.EMZ:
          case FileType.GIF:
          case FileType.JPG:
          case FileType.JPEG:
          case FileType.TIF:
          case FileType.TIFF:
          case FileType.PNG:
            converterType = SyncFusionPdfConverterType.SyncFusionImaging;
            conversionResult = _imagingPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;

          case FileType.PDF:
            converterType = SyncFusionPdfConverterType.SyncFusionPdf;
            conversionResult = _pdfRendererService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;

          case FileType.XPS:
            converterType = SyncFusionPdfConverterType.SyncFusionXps;
            conversionResult = _xpsPdfRendererService.ReadToPdfStream(inputStream, documentId, correlationId);
            break;
          default:
            conversionResult = new SyncFusionPdfConversionResult(documentId, converterType);
            conversionResult.RecordConversionFailure(PdfConversionStatus.DocumentTypeUnsupported, $"Unsupported file type: {fileType}");
            break;
        }
      }
      catch (Exception ex)
      {
        inputStream?.Dispose();
        conversionResult = new SyncFusionPdfConversionResult(documentId, converterType);
        conversionResult.RecordConversionFailure(PdfConversionStatus.UnexpectedError, ex.ToFormattedString());

        return conversionResult;
      }
      return conversionResult;
    }
  }

}