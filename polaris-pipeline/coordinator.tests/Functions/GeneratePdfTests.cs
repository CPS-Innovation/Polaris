using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Handlers.Contracts;
using Common.Services.BlobStorageService.Contracts;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Document;
using DdeiClient.Services.Contracts;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace pdf_generator.tests.Functions
{
    public class GeneratePdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedGeneratePdfRequest;
        private readonly CaseDocumentOrchestrationPayload _generatePdfRequest;
        private readonly string _blobName;
        private readonly Stream _documentStream;
        private readonly Stream _pdfStream;
        private readonly string _serializedGeneratePdfResponse;

        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IDdeiClient> _mockDocumentExtractionService;
        private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<GeneratePdf>> _mockLogger;
        private readonly Mock<IValidatorWrapper<CaseDocumentOrchestrationPayload>> _mockValidatorWrapper;
        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly HttpClient _httpClient;

        private readonly GeneratePdf _generatePdf;

        public GeneratePdfTests()
        {
            _serializedGeneratePdfRequest = _fixture.Create<string>();
            var cmsAuthValues = _fixture.Create<string>();
            _generatePdfRequest = _fixture.Build<CaseDocumentOrchestrationPayload>()
                                    .With(r => r.CmsFileName, "Test.doc")
                                    .With(r => r.CmsCaseId, 123456)
                                    .With(r => r.CmsVersionId, 654321)
                                    .Create();
            _blobName = $"{_generatePdfRequest.CmsCaseId}/pdfs/{Path.GetFileNameWithoutExtension(_generatePdfRequest.CmsFileName)}.pdf";
            _fixture.Create<string>();
            _documentStream = new MemoryStream();
            _pdfStream = new MemoryStream();
            _serializedGeneratePdfResponse = _fixture.Create<string>();

            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockValidatorWrapper = new Mock<IValidatorWrapper<CaseDocumentOrchestrationPayload>>();
            _mockDocumentExtractionService = new Mock<IDdeiClient>();
            _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _mockLogger = new Mock<ILogger<GeneratePdf>>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            // https://carlpaton.github.io/2021/01/mocking-httpclient-sendasync/
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            httpMessageHandlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(nameof(HttpClient.SendAsync), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            _httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://base.url/") };
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            _mockJsonConvertWrapper
                .Setup(wrapper => wrapper.DeserializeObject<CaseDocumentOrchestrationPayload>(_serializedGeneratePdfRequest))
                .Returns(_generatePdfRequest);
            _mockJsonConvertWrapper
                .Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfResponse>(r => r.BlobName == _blobName)))
                .Returns(_serializedGeneratePdfResponse);
            _mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_generatePdfRequest)).Returns(new List<ValidationResult>());
            _mockDocumentExtractionService
                .Setup(service => service.GetDocumentAsync(_generatePdfRequest.CmsCaseUrn, _generatePdfRequest.CmsCaseId.ToString(),
                    _generatePdfRequest.CmsDocumentCategory, _generatePdfRequest.CmsDocumentId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(_documentStream);
            _mockHttpClientFactory
                .Setup(httpClientFactory => httpClientFactory.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);
            _mockDurableActivityContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns(_generatePdfRequest);

            _generatePdf = new GeneratePdf(
                                _mockHttpClientFactory.Object,
                                _mockJsonConvertWrapper.Object,
                                _mockValidatorWrapper.Object,
                                _mockDocumentExtractionService.Object,
                                _mockBlobStorageService.Object,
                                _mockExceptionHandler.Object,
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
            _generatePdfRequest.CmsFileName = "Test.pdf";
            _mockDocumentExtractionService
                .Setup(service => service.GetDocumentAsync
                (
                    _generatePdfRequest.CmsCaseUrn, 
                    _generatePdfRequest.CmsCaseId.ToString(),
                    _generatePdfRequest.CmsDocumentCategory, 
                    _generatePdfRequest.CmsDocumentId, 
                    It.IsAny<string>(), 
                    It.IsAny<Guid>())
                )
                .ReturnsAsync(_documentStream);

            await _generatePdf.Run(_mockDurableActivityContext.Object);

            _mockBlobStorageService.Verify
            (
                service => service.UploadDocumentAsync
                (
                    It.IsAny<Stream>(), 
                    _blobName, 
                    _generatePdfRequest.CmsCaseId.ToString(), 
                    _generatePdfRequest.CmsDocumentId,
                    _generatePdfRequest.CmsVersionId.ToString(),
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
                    It.IsAny<Stream>(), 
                    _blobName, 
                    _generatePdfRequest.CmsCaseId.ToString(), 
                    _generatePdfRequest.CmsDocumentId,
                    _generatePdfRequest.CmsVersionId.ToString(),
                    _generatePdfRequest.CorrelationId
                )
            );
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
            var response = await _generatePdf.Run(_mockDurableActivityContext.Object);

            response.AlreadyProcessed.Should().BeFalse();
            response.BlobName.Should().Be( _blobName );
        }
    }
}