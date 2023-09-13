using System;
using System.IO;
using AutoFixture;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
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
		private readonly Mock<IPdfService> _mockCellsPdfService;
		private readonly Mock<IPdfService> _mockSlidesPdfService;
		private readonly Mock<IPdfService> _mockImagingPdfService;
		private readonly Mock<IPdfService> _mockDiagramPdfService;
		private readonly Mock<IPdfService> _mockHtmlPdfService;
		private readonly Mock<IPdfService> _mockEmailPdfService;
		private readonly Mock<IPdfService> _mockPdfRendererService;
        private readonly Mock<IConfigurationRoot> _configuration;

        private readonly IPdfOrchestratorService _pdfOrchestratorService;

		public PdfOrchestratorServiceTests()
		{
			var fixture = new Fixture();
			_inputStream = new MemoryStream();
			_documentId = fixture.Create<string>();
			_correlationId = fixture.Create<Guid>();

			_mockWordsPdfService = new Mock<IPdfService>();
			_mockCellsPdfService = new Mock<IPdfService>();
			_mockSlidesPdfService = new Mock<IPdfService>();
			_mockImagingPdfService = new Mock<IPdfService>();
			_mockDiagramPdfService = new Mock<IPdfService>();
			_mockHtmlPdfService = new Mock<IPdfService>();
			_mockEmailPdfService = new Mock<IPdfService>();
			_mockPdfRendererService = new Mock<IPdfService>();
			_configuration = new Mock<IConfigurationRoot>();
			var mockLogger = new Mock<ILogger<PdfOrchestratorService>>();

			_pdfOrchestratorService = new PdfOrchestratorService(
										_mockWordsPdfService.Object,
										_mockCellsPdfService.Object,
										_mockSlidesPdfService.Object,
										_mockImagingPdfService.Object,
										_mockDiagramPdfService.Object,
										_mockHtmlPdfService.Object,
										_mockEmailPdfService.Object,
										_mockPdfRendererService.Object,
										mockLogger.Object,
										_configuration.Object);
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsDoc()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.DOC, _documentId, _correlationId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsDocx()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.DOCX, _documentId, _correlationId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsDocm()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.DOCM, _documentId, _correlationId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsRtf()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.RTF, _documentId, _correlationId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsTxt()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.TXT, _documentId, _correlationId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsCellsServiceWhenFileTypeIsXls()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.XLS, _documentId, _correlationId);

			_mockCellsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsCellsServiceWhenFileTypeIsXlsx()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.XLSX, _documentId, _correlationId);

			_mockCellsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsSlidesServiceWhenFileTypeIsPpt()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PPT, _documentId, _correlationId);

			_mockSlidesPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsSlidesServiceWhenFileTypeIsPptx()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PPTX, _documentId, _correlationId);

			_mockSlidesPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsBmp()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.BMP, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsGif()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.GIF, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsJpg()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.JPG, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsJpeg()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.JPEG, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsTif()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.TIF, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsTiff()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.TIFF, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsPng()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PNG, _documentId, _correlationId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsDiagramServiceWhenFileTypeIsVsd()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.VSD, _documentId, _correlationId);

			_mockDiagramPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsHtmlServiceWhenFileTypeIsHtml()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.HTML, _documentId, _correlationId);

			_mockHtmlPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsHtmlServiceWhenFileTypeIsHtm()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.HTM, _documentId, _correlationId);

			_mockHtmlPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsEmailServiceWhenFileTypeIsMsg()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.MSG, _documentId, _correlationId);

			_mockEmailPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsPdfRendererServiceWhenFileTypeIsPdf()
		{
			_pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PDF, _documentId, _correlationId);

			_mockPdfRendererService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()));
		}

        [Fact]
        public void ReadToPdfStream_CallsPdfRendererServiceWhenFileTypeIsHte_AndFeatureIsEnabled()
        {
			// Arrange
			_configuration.SetupGet(config => config[FeatureFlags.HteFeatureFlag]).Returns("true");

			// Act
            _pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.HTE, _documentId, _correlationId);

            // Assert
            _mockHtmlPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void ReadToPdfStream_DoesntCallPdfRendererServiceWhenFileTypeIsHte_AndFeatureIsDisabled()
        {
			// Arrange
            _configuration.SetupGet(config => config[FeatureFlags.HteFeatureFlag]).Returns("false");

			// Act
            _pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.HTE, _documentId, _correlationId);

            // Assert
            _mockHtmlPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
		public void ReadToPdfStream_ThrowsFailedToConvertToPdfExceptionWhenExceptionOccurs()
		{
			_mockEmailPdfService.Setup(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>(), It.IsAny<Guid>()))
				.Throws(new Exception());

			Assert.Throws<PdfConversionException>(() => _pdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.MSG, _documentId, _correlationId));
		}
	}
}

