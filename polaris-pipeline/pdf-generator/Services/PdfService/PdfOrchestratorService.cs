using System;
using System.IO;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Logging;
using Microsoft.Extensions.Configuration;
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
        private readonly IPdfService _htmlPdfService;
        private readonly IPdfService _emailPdfService;
        private readonly IPdfService _pdfRendererService;
        private readonly ILogger<PdfOrchestratorService> _logger;
        private readonly IConfiguration _configuration;

        public PdfOrchestratorService(
            IPdfService wordsPdfService,
            IPdfService cellsPdfService,
            IPdfService slidesPdfService,
            IPdfService imagingPdfService,
            IPdfService diagramPdfService,
            IPdfService htmlPdfService,
            IPdfService emailPdfService,
            IPdfService pdfRendererService,
            ILogger<PdfOrchestratorService> logger,
            IConfiguration configuration)
        {
            _wordsPdfService = wordsPdfService;
            _cellsPdfService = cellsPdfService;
            _slidesPdfService = slidesPdfService;
            _imagingPdfService = imagingPdfService;
            _diagramPdfService = diagramPdfService;
            _htmlPdfService = htmlPdfService;
            _emailPdfService = emailPdfService;
            _pdfRendererService = pdfRendererService;
            _logger = logger;
            _configuration = configuration;
        }

        public Stream ReadToPdfStream(Stream inputStream, FileType fileType, string documentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(ReadToPdfStream), documentId);

            try
            {
                _logger.LogMethodFlow(correlationId, nameof(ReadToPdfStream), "Analysing file type and matching to a converter");
                var pdfStream = new MemoryStream();
                switch (fileType)
                {
                    case FileType.DOC:
                    case FileType.DOCX:
                    case FileType.DOCM:
                    case FileType.RTF:
                    case FileType.TXT:
                        _wordsPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.XLS:
                    case FileType.XLSX:
                        _cellsPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.PPT:
                    case FileType.PPTX:
                        _slidesPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.BMP:
                    case FileType.GIF:
                    case FileType.JPG:
                    case FileType.JPEG:
                    case FileType.TIF:
                    case FileType.TIFF:
                    case FileType.PNG:
                        _imagingPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.VSD:
                        _diagramPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.HTML:
                    case FileType.HTM:
                        _htmlPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    // CMS HTE format is a custom HTML format, with a pre-<HTML> set of <b> tag metadata headers (i.e. not standard HTML)
                    // But Aspose seems forgiving enough to convert it, so treat it as HTML
                    case FileType.HTE:
                        _htmlPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.MSG:
                        _emailPdfService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    case FileType.PDF:
                        _pdfRendererService.ReadToPdfStream(inputStream, pdfStream, correlationId);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
                }

                return pdfStream;
            }
            catch (Exception exception)
            {
                inputStream?.Dispose();

                //the stack trace is lost here if simply thrown but not preserved except the message - ensure the exception is logged in full here until exceptions are reworked in general
                _logger.LogMethodError(correlationId, nameof(ReadToPdfStream), exception.Message, exception);
                throw new PdfConversionException(documentId, exception.Message);
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(ReadToPdfStream), string.Empty);
            }
        }
    }
}
