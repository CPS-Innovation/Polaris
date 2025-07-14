using System;
using System.IO;
using AutoFixture;
using Common.Domain.Document;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
    public class PdfOrchestratorServiceTests
    {
        private readonly Stream _inputStream;
        private readonly string _documentId;
        private readonly Guid _correlationId;

        private readonly Mock<IPdfService> _mockWordsPdfService;
        private readonly Mock<IPdfService> _mockHtePdfService;
        private readonly Mock<IPdfService> _mockCellsPdfService;
        private readonly Mock<IPdfService> _mockSlidesPdfService;
        private readonly Mock<IPdfService> _mockImagingPdfService;
        private readonly Mock<IPdfService> _mockDiagramPdfService;
        private readonly Mock<IPdfService> _mockEmailPdfService;
        private readonly Mock<IPdfService> _mockPdfRendererService;
        private readonly Mock<IPdfService> _mockXpsPdfRendererService;

        private readonly IPdfOrchestratorService _pdfOrchestratorService;

        public PdfOrchestratorServiceTests()
        {
            var fixture = new Fixture();
            _inputStream = new MemoryStream();
            _documentId = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();

            _mockWordsPdfService = new Mock<IPdfService>();
            _mockHtePdfService = new Mock<IPdfService>();
            _mockCellsPdfService = new Mock<IPdfService>();
            _mockSlidesPdfService = new Mock<IPdfService>();
            _mockImagingPdfService = new Mock<IPdfService>();
            _mockDiagramPdfService = new Mock<IPdfService>();
            _mockEmailPdfService = new Mock<IPdfService>();
            _mockPdfRendererService = new Mock<IPdfService>();
            _mockXpsPdfRendererService = new Mock<IPdfService>();
            var mockLogger = new Mock<ILogger<PdfOrchestratorService>>();

            _pdfOrchestratorService = new PdfOrchestratorService(
                                        _mockWordsPdfService.Object,
                                        _mockHtePdfService.Object,
                                        _mockCellsPdfService.Object,
                                        _mockSlidesPdfService.Object,
                                        _mockImagingPdfService.Object,
                                        _mockDiagramPdfService.Object,
                                        _mockEmailPdfService.Object,
                                        _mockPdfRendererService.Object,
                                        _mockXpsPdfRendererService.Object,
                                        mockLogger.Object);
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsDoc()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.DOC, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsDocx()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.DOCX, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsDocm()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.DOCM, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsRtf()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.RTF, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsTxt()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.TXT, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsCellsServiceWhenFileTypeIsXls()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.XLS, _documentId, _correlationId);

            _mockCellsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsCellsServiceWhenFileTypeIsXlsx()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.XLSX, _documentId, _correlationId);

            _mockCellsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsSlidesServiceWhenFileTypeIsPpt()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.PPT, _documentId, _correlationId);

            _mockSlidesPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsSlidesServiceWhenFileTypeIsPptx()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.PPTX, _documentId, _correlationId);

            _mockSlidesPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsBmp()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.BMP, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsGif()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.GIF, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsJpg()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.JPG, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsJpeg()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.JPEG, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsTif()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.TIF, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsTiff()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.TIFF, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsImagingServiceWhenFileTypeIsPng()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.PNG, _documentId, _correlationId);

            _mockImagingPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsDiagramServiceWhenFileTypeIsVsd()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.VSD, _documentId, _correlationId);

            _mockDiagramPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsHtmlServiceWhenFileTypeIsHtml()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.HTML, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsHtmlServiceWhenFileTypeIsHtm()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.HTM, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsEmailServiceWhenFileTypeIsMsg()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.MSG, _documentId, _correlationId);

            _mockEmailPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsPdfRendererServiceWhenFileTypeIsPdf()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.PDF, _documentId, _correlationId);

            _mockPdfRendererService.Verify(service => service.ReadToPdfStreamAsync(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsHte()
        {
            // Act
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.HTE, _documentId, _correlationId);

            // Assert
            _mockHtePdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsMht()
        {
            // Act
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.MHT, _documentId, _correlationId);

            // Assert
            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsMhtml()
        {
            // Act
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.MHTML, _documentId, _correlationId);

            // Assert
            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsDot()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.DOT, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsDotm()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.DOTM, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsWordsServiceWhenFileTypeIsDotx()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.DOTX, _documentId, _correlationId);

            _mockWordsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsCellsServiceWhenFileTypeIsXlsm()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.XLSM, _documentId, _correlationId);

            _mockCellsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsCellsServiceWhenFileTypeIsXlt()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.XLT, _documentId, _correlationId);

            _mockCellsPdfService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public void ReadToPdfStreamAsync_CallsCellsServiceWhenFileTypeIsXps()
        {
            _pdfOrchestratorService.ReadToPdfStreamAsync(_inputStream, FileType.XPS, _documentId, _correlationId);

            _mockXpsPdfRendererService.Verify(service => service.ReadToPdfStream(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }
    }
}

