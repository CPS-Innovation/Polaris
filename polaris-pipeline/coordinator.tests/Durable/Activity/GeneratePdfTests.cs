using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Common.Clients.PdfGenerator;
using Common.Domain.Document;
using Common.Dto.Tracker;
using Common.Services.BlobStorage;
using coordinator.Durable.Activity;
using Ddei;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Moq.Protected;
using Xunit;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Common.Constants;
using FluentAssertions;
using Ddei.Factories;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Dto.Response.Document;

namespace pdf_generator.tests.Durable.Activity
{
    public class GeneratePdfFromDocumentTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedGeneratePdfRequest;
        private readonly DocumentPayload _generatePdfRequest;
        private readonly Stream _pdfStream;
        private readonly string _serializedGeneratePdfResponse;
        private readonly Mock<IDdeiClient> _mockDDeiClient;
        private readonly Mock<IDdeiArgFactory> _mockDdeiArgFactory;
        private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;
        private readonly Mock<IPdfGeneratorClient> _mockPdfGeneratorClient;
        private readonly GeneratePdfFromDocument _generatePdf;

        public GeneratePdfFromDocumentTests()
        {
            _serializedGeneratePdfRequest = _fixture.Create<string>();


            var trackerCmsDocumentDto = _fixture.Create<DocumentDto>();
            _generatePdfRequest = new DocumentPayload
                (
                    _fixture.Create<string>(),
                    _fixture.Create<int>(),
                    _fixture.Create<string>(),
                    _fixture.Create<long>(),
                    _fixture.Create<string>(),
                    _fixture.Create<DocumentTypeDto>(),
                    DocumentNature.Document,
                    DocumentDeltaType.RequiresIndexing,
                    _fixture.Create<string>(),
                    _fixture.Create<Guid>()
                );


            var docxStream = new MemoryStream();
            var pdfStream = new MemoryStream();

            _pdfStream = new MemoryStream();
            _serializedGeneratePdfResponse = _fixture.Create<string>();


            _mockDDeiClient = new Mock<IDdeiClient>();
            _mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
            _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();

            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDDeiClient
                .Setup(service => service.GetDocumentFromFileStoreAsync
                (
                    "test.docx",
                    It.IsAny<string>(),
                    It.IsAny<Guid>())
                )
                .ReturnsAsync(docxStream);

            _mockDDeiClient
                .Setup(service => service.GetDocumentFromFileStoreAsync
                (
                    "test.pdf",
                    It.IsAny<string>(),
                    It.IsAny<Guid>())
                )
                .ReturnsAsync(pdfStream);

            _mockDurableActivityContext
                .Setup(context => context.GetInput<DocumentPayload>())
                .Returns(_generatePdfRequest);

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

            _generatePdf = new GeneratePdfFromDocument(
                                _mockPdfGeneratorClient.Object,
                                _mockDDeiClient.Object,
                                _mockDdeiArgFactory.Object,
                                _mockBlobStorageService.Object);
        }

        [Fact]
        public async Task Run_ReturnsUnsupportedWhenFileTypeIsUnrecognised()
        {
            _generatePdfRequest.Path = null;
            _mockDurableActivityContext
                .Setup(context => context.GetInput<DocumentPayload>())
                .Returns(_generatePdfRequest);
            var result = await _generatePdf.Run(_mockDurableActivityContext.Object);
            result.Should().Be(PdfConversionStatus.DocumentTypeUnsupported);
        }

        [Fact]
        public async Task Run_UploadsDocumentStreamWhenFileTypeIsPdf()
        {
            _generatePdfRequest.Path = "test.pdf";
            await _generatePdf.Run(_mockDurableActivityContext.Object);

            _mockBlobStorageService.Verify
            (
                service => service.UploadBlobAsync
                (
                    _pdfStream,
                    It.IsAny<BlobIdType>()
                )
            );
        }

        [Fact]
        public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
        {
            _generatePdfRequest.Path = "test.docx";
            await _generatePdf.Run(_mockDurableActivityContext.Object);

            _mockBlobStorageService.Verify
            (
                service => service.UploadBlobAsync
                (
                    _pdfStream,
                    It.IsAny<BlobIdType>()
                )
            );
        }
    }
}