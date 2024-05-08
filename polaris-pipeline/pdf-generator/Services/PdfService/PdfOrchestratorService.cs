using System;
using System.IO;
using Common.Domain.Document;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.PdfService
{
    public class PdfOrchestratorService : IPdfOrchestratorService
    {
        private readonly IPdfService _wordsPdfService;
        private readonly IPdfService _cellsPdfService;
        private readonly IPdfService _slidesPdfService;
        private readonly IPdfService _imagingPdfService;
        private readonly IPdfService _diagramPdfService;
        private readonly IPdfService _emailPdfService;
        private readonly IPdfService _pdfRendererService;
        private readonly IPdfService _xpsPdfRendererService;
        private readonly ILogger<PdfOrchestratorService> _logger;

        public PdfOrchestratorService(
            IPdfService wordsPdfService,
            IPdfService cellsPdfService,
            IPdfService slidesPdfService,
            IPdfService imagingPdfService,
            IPdfService diagramPdfService,
            IPdfService emailPdfService,
            IPdfService pdfRendererService,
            IPdfService xpsPdfRendererService,
            ILogger<PdfOrchestratorService> logger)
        {
            _wordsPdfService = wordsPdfService;
            _cellsPdfService = cellsPdfService;
            _slidesPdfService = slidesPdfService;
            _imagingPdfService = imagingPdfService;
            _diagramPdfService = diagramPdfService;
            _emailPdfService = emailPdfService;
            _pdfRendererService = pdfRendererService;
            _xpsPdfRendererService = xpsPdfRendererService;
            _logger = logger;
        }

        public PdfConversionResult ReadToPdfStream(Stream inputStream, FileType fileType, string documentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(ReadToPdfStream), documentId);
            PdfConversionResult conversionResult;
            var converterType = PdfConverterType.None;

            try
            {
                _logger.LogMethodFlow(correlationId, nameof(ReadToPdfStream),
                    "Analysing file type and matching to a converter");
                switch (fileType)
                {
                    case FileType.DOC:
                    case FileType.DOCX:
                    case FileType.DOCM:
                    case FileType.DOT:
                    case FileType.DOTM:
                    case FileType.DOTX:
                    case FileType.RTF:
                    case FileType.TXT:
                    case FileType.HTML:
                    case FileType.HTM:
                    case FileType.MHT:
                    case FileType.MHTML:
                    // CMS HTE format is a custom HTML format, with a pre-<HTML> set of <b> tag metadata headers (i.e. not standard HTML)
                    // But Aspose seems forgiving enough to convert it, so treat it as HTML
                    case FileType.HTE:
                        converterType = PdfConverterType.AsposeWords;
                        conversionResult = _wordsPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    case FileType.CSV:
                    case FileType.XLS:
                    case FileType.XLSX:
                    case FileType.XLSM:
                    case FileType.XLT:
                        converterType = PdfConverterType.AsposeCells;
                        conversionResult = _cellsPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    case FileType.PPT:
                    case FileType.PPTX:
                        converterType = PdfConverterType.AsposeSlides;
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
                        converterType = PdfConverterType.AsposeImaging;
                        conversionResult = _imagingPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    case FileType.VSD:
                        converterType = PdfConverterType.AsposeDiagrams;
                        conversionResult = _diagramPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    case FileType.EML:
                    case FileType.MSG:
                        converterType = PdfConverterType.AsposeEmail;
                        conversionResult = _emailPdfService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    case FileType.PDF:
                        converterType = PdfConverterType.AsposePdf;
                        conversionResult = _pdfRendererService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    case FileType.XPS:
                        converterType = PdfConverterType.AsposePdf;
                        conversionResult = _xpsPdfRendererService.ReadToPdfStream(inputStream, documentId, correlationId);
                        break;

                    default:
                        conversionResult = new PdfConversionResult(documentId, PdfConverterType.None);
                        conversionResult.RecordConversionFailure(PdfConversionStatus.DocumentTypeUnsupported, $"File type {nameof(fileType)} is currently unsupported by Polaris");
                        return conversionResult;
                }
            }
            catch (Exception exception)
            {
                inputStream?.Dispose();

                _logger.LogMethodError(correlationId, nameof(ReadToPdfStream), exception.Message, exception);
                conversionResult = new PdfConversionResult(documentId, converterType);
                conversionResult.RecordConversionFailure(PdfConversionStatus.UnexpectedError, exception.ToFormattedString());

                return conversionResult;
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(ReadToPdfStream), string.Empty);
            }

            return conversionResult;
        }
    }
}
