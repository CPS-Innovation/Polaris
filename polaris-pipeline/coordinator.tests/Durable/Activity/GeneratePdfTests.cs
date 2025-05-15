using AutoFixture;
using Common.Clients.PdfGenerator;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Response;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace pdf_generator.tests.Durable.Activity
{
    public class GeneratePdfFromDocumentTests
    {
        private readonly Fixture _fixture = new();
        private readonly DocumentPayload _generatePdfRequest;
        private readonly Stream _pdfStream;
        private readonly Mock<IDdeiClient> _mockDDeiClient;
        private readonly Mock<IDdeiArgFactory> _mockDdeiArgFactory;
        private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
        private readonly Mock<IPdfGeneratorClient> _mockPdfGeneratorClient;
        private readonly GeneratePdfFromDocument _generatePdf;

        public GeneratePdfFromDocumentTests()
        {
            var trackerCmsDocumentDto = _fixture.Create<DocumentDto>();
            _generatePdfRequest = new DocumentPayload
                (
                    _fixture.Create<string>(),
                    _fixture.Create<int>(),
                    _fixture.Create<string>(),
                    _fixture.Create<long>(),
                    _fixture.Create<string>(),
                    _fixture.Create<DocumentTypeDto>(),
                    DocumentNature.Types.Document,
                    DocumentDeltaType.RequiresIndexing,
                    _fixture.Create<string>(),
                    _fixture.Create<Guid>());
            var docxStream = new MemoryStream();
            var pdfStream = new MemoryStream();
            _pdfStream = new MemoryStream();
            _mockDDeiClient = new Mock<IDdeiClient>();
            _mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
            _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();

            var docArg = _fixture.Create<DdeiDocumentIdAndVersionIdArgDto>();
            _mockDdeiArgFactory
                .Setup(x => x.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(docArg);

            _mockDDeiClient
               .Setup(service => service.GetDocumentAsync(docArg))
                .ReturnsAsync(new FileResult { Stream = docxStream });

            _mockPdfGeneratorClient = new Mock<IPdfGeneratorClient>();

            _mockPdfGeneratorClient
                .Setup(client => client.ConvertToPdfAsync(
                    _generatePdfRequest.CorrelationId,
                    _generatePdfRequest.Urn,
                    _generatePdfRequest.CaseId,
                    _generatePdfRequest.DocumentId,
                    _generatePdfRequest.VersionId,
                    pdfStream,
                    FileType.PDF))
                .ReturnsAsync(new ConvertToPdfResponse() { PdfStream = _pdfStream, Status = PdfConversionStatus.DocumentConverted });

            _mockPdfGeneratorClient
                .Setup(client => client.ConvertToPdfAsync(
                    _generatePdfRequest.CorrelationId,
                    _generatePdfRequest.Urn,
                    _generatePdfRequest.CaseId,
                    _generatePdfRequest.DocumentId,
                    _generatePdfRequest.VersionId,
                    docxStream,
                    FileType.DOCX))
                .ReturnsAsync(new ConvertToPdfResponse() { PdfStream = _pdfStream, Status = PdfConversionStatus.DocumentConverted });

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x[StorageKeys.BlobServiceContainerNameDocuments]).Returns("Documents");

            var mockStorageDelegate = new Mock<Func<string, IPolarisBlobStorageService>>();
            mockStorageDelegate.Setup(s => s("Documents")).Returns(_mockBlobStorageService.Object);

            _generatePdf = new GeneratePdfFromDocument(
                                _mockPdfGeneratorClient.Object,
                                _mockDDeiClient.Object,
                                _mockDdeiArgFactory.Object,
                                mockStorageDelegate.Object,
                                mockConfiguration.Object);
        }

        [Fact]
        public async Task Run_ReturnsUnsupportedWhenFileTypeIsUnrecognised()
        {
            _generatePdfRequest.Path = null;
            var result = await _generatePdf.Run(_generatePdfRequest);
            result.Should().BeEquivalentTo(new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = PdfConversionStatus.DocumentTypeUnsupported });
        }

        [Fact]
        public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
        {
            _generatePdfRequest.Path = "test.docx";
            await _generatePdf.Run(_generatePdfRequest);

            _mockBlobStorageService.Verify
            (
                service => service.UploadBlobAsync
                (
                    _pdfStream,
                    It.IsAny<BlobIdType>(),
                    null
                )
            );
        }
    }
}