using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Clients.Contracts;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Dto.Tracker;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Services.RenderHtmlService.Contract;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using coordinator.Durable.Activity;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;

namespace pdf_generator.tests.Functions
{
    public class GeneratePdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedGeneratePdfRequest;
        private readonly CaseDocumentOrchestrationPayload _generatePdfRequest;
        private readonly Stream _documentStream;
        private readonly Stream _pdfStream;
        private readonly string _serializedGeneratePdfResponse;
        private readonly Mock<IConvertModelToHtmlService> _mockConvertPcdRequestToHtmlService;
        private readonly Mock<IDdeiClient> _mockDDeiClient;
        private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
        private readonly Mock<ILogger<GeneratePdf>> _mockLogger;
        private readonly Mock<IValidatorWrapper<CaseDocumentOrchestrationPayload>> _mockValidatorWrapper;
        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;
        private readonly Mock<IPdfGeneratorClient> _mockPdfGeneratorClient;
        private readonly GeneratePdf _generatePdf;

        public GeneratePdfTests()
        {
            _serializedGeneratePdfRequest = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();
            var trackerCmsDocumentDto = _fixture.Create<DocumentDto>();
            _generatePdfRequest = new CaseDocumentOrchestrationPayload
                (
                    _fixture.Create<string>(),
                    Guid.NewGuid(),
                    _fixture.Create<string>(),
                    _fixture.Create<long>(),
                    JsonSerializer.Serialize(trackerCmsDocumentDto),
                    null,
                    null,
                    DocumentDeltaType.RequiresIndexing
                );
            _generatePdfRequest.CmsCaseId = 123456;
            _generatePdfRequest.CmsDocumentTracker.CmsOriginalFileExtension = ".doc";
            _generatePdfRequest.CmsDocumentTracker.PresentationTitle = "Test document";
            _generatePdfRequest.CmsDocumentTracker.CmsOriginalFileName = "Test.doc";
            _generatePdfRequest.CmsDocumentTracker.CmsVersionId = 654321;
            _generatePdfRequest.CmsDocumentTracker.CmsDocumentId = _fixture.Create<string>();

            _documentStream = new MemoryStream();
            _pdfStream = new MemoryStream();
            _serializedGeneratePdfResponse = _fixture.Create<string>();

            _mockConvertPcdRequestToHtmlService = new Mock<IConvertModelToHtmlService>();

            _mockValidatorWrapper = new Mock<IValidatorWrapper<CaseDocumentOrchestrationPayload>>();
            _mockDDeiClient = new Mock<IDdeiClient>();
            _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
            _mockLogger = new Mock<ILogger<GeneratePdf>>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            // https://carlpaton.github.io/2021/01/mocking-httpclient-sendasync/
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            httpMessageHandlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(nameof(HttpClient.SendAsync), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            _mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_generatePdfRequest)).Returns(new List<ValidationResult>());
            _mockDDeiClient
                .Setup(service => service.GetDocumentFromFileStoreAsync
                (
                    _generatePdfRequest.CmsDocumentTracker.Path,
                    It.IsAny<string>(),
                    It.IsAny<Guid>())
                )
                .ReturnsAsync(_documentStream);

            _mockDurableActivityContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns(_generatePdfRequest);

            _mockPdfGeneratorClient = new Mock<IPdfGeneratorClient>();

            _mockPdfGeneratorClient
                .Setup(client => client.ConvertToPdfAsync(
                    _generatePdfRequest.CorrelationId,
                    _generatePdfRequest.CmsAuthValues,
                    _generatePdfRequest.CmsCaseUrn,
                    _generatePdfRequest.CmsCaseId.ToString(),
                    _generatePdfRequest.CmsDocumentId,
                    _generatePdfRequest.CmsVersionId.ToString(),
                    _documentStream,
                    FileType.DOC))
                .ReturnsAsync(_pdfStream);

            _generatePdf = new GeneratePdf(
                                _mockConvertPcdRequestToHtmlService.Object,
                                _mockPdfGeneratorClient.Object,
                                _mockValidatorWrapper.Object,
                                _mockDDeiClient.Object,
                                _mockBlobStorageService.Object,
                                _mockLogger.Object);
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenPayloadIsNull()
        {
            _mockDurableActivityContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns((CaseDocumentOrchestrationPayload)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _generatePdf.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
        {
            var validationResults = _fixture.CreateMany<ValidationResult>(2).ToList();

            _mockValidatorWrapper
                .Setup(wrapper => wrapper.Validate(It.IsAny<CaseDocumentOrchestrationPayload>()))
                .Returns(validationResults);

            await Assert.ThrowsAsync<BadRequestException>(() => _generatePdf.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_UploadsDocumentStreamWhenFileTypeIsPdf()
        {
            _generatePdfRequest.CmsDocumentTracker.PresentationTitle = "Test.pdf";
            await _generatePdf.Run(_mockDurableActivityContext.Object);

            _mockBlobStorageService.Verify
            (
                service => service.UploadDocumentAsync
                (
                    _pdfStream,
                    _generatePdfRequest.BlobName,
                    _generatePdfRequest.CmsCaseId.ToString(),
                    _generatePdfRequest.CmsDocumentTracker.PolarisDocumentId,
                    _generatePdfRequest.CmsDocumentTracker.CmsVersionId.ToString(),
                    _generatePdfRequest.CorrelationId
                )
            );
        }

        [Fact]
        public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
        {
            await _generatePdf.Run(_mockDurableActivityContext.Object);

            _mockBlobStorageService.Verify
            (
                service => service.UploadDocumentAsync
                (
                    _pdfStream,
                    _generatePdfRequest.BlobName,
                    _generatePdfRequest.CmsCaseId.ToString(),
                    _generatePdfRequest.CmsDocumentTracker.PolarisDocumentId,
                    _generatePdfRequest.CmsDocumentTracker.CmsVersionId.ToString(),
                    _generatePdfRequest.CorrelationId
                )
            );
        }
    }
}