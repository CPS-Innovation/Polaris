using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Common.Domain.Exceptions;
using Common.Services.CaseSearchService.Contracts;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using Common.Services.OcrService;
using Xunit;
using Common.Wrappers.Contracts;
using Common.Dto.Request;
using Common.Handlers.Contracts;
using Common.Telemetry.Contracts;
using System.IO;
using Common.Mappers.Contracts;

namespace text_extractor.tests.Functions
{
    public class ExtractTextTests
    {
        private readonly Fixture _fixture;
        private readonly string _serializedExtractTextRequest;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly ExtractTextRequestDto _extractTextRequest;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly AnalyzeResults _mockAnalyzeResults;
        private readonly Mock<IValidatorWrapper<ExtractTextRequestDto>> _mockValidatorWrapper;

        private readonly Mock<ILogger<ExtractText>> _mockLogger;

        private readonly Mock<ITelemetryClient> _mockTelemetryClient;
        private readonly Guid _correlationId;

        private readonly ExtractText _extractText;

        private List<ValidationResult> _validationResults;
        public ExtractTextTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _serializedExtractTextRequest = _fixture.Create<string>();
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(_serializedExtractTextRequest)
            };
            _extractTextRequest = _fixture.Create<ExtractTextRequestDto>();
            _mockValidatorWrapper = new Mock<IValidatorWrapper<ExtractTextRequestDto>>();
            var mockOcrService = new Mock<IOcrService>();
            _mockSearchIndexService = new Mock<ISearchIndexService>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _mockAnalyzeResults = Mock.Of<AnalyzeResults>(ctx => ctx.ReadResults == new List<ReadResult>());
            _mockTelemetryClient = new Mock<ITelemetryClient>();

            _correlationId = _fixture.Create<Guid>();

            _validationResults = new List<ValidationResult>();
            _mockValidatorWrapper
                .Setup(wrapper => wrapper.Validate(_extractTextRequest))
                .Returns(_validationResults);
            mockOcrService.Setup(service => service.GetOcrResultsAsync(It.IsAny<Stream>(), It.IsAny<Guid>()))
                .ReturnsAsync(_mockAnalyzeResults);

            _mockLogger = new Mock<ILogger<ExtractText>>();

            _mockSearchIndexService
                .Setup(service => service.WaitForStoreResultsAsync(It.IsAny<AnalyzeResults>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(true));

            var mockDtoHttpRequestHeadersMapper = new Mock<IDtoHttpRequestHeadersMapper>();

            mockDtoHttpRequestHeadersMapper.Setup(mapper => mapper.Map<ExtractTextRequestDto>(It.IsAny<HttpHeaders>()))
                .Returns(_extractTextRequest);

            _extractText = new ExtractText(
                                _mockValidatorWrapper.Object,
                                mockOcrService.Object,
                                _mockSearchIndexService.Object,
                                _mockExceptionHandler.Object,
                                mockDtoHttpRequestHeadersMapper.Object,
                                _mockLogger.Object,
                                _mockTelemetryClient.Object);
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<ExtractText>>()))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _extractText.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);

            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _validationResults.Add(new ValidationResult("Invalid"));
            var response = await _extractText.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

            var response = await _extractText.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

            var response = await _extractText.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_StoresOcrResults()
        {
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            await _extractText.Run(_httpRequestMessage);

            _mockSearchIndexService.Verify(service => service.SendStoreResultsAsync(_mockAnalyzeResults, _extractTextRequest.PolarisDocumentId, _extractTextRequest.CaseId, _extractTextRequest.DocumentId,
                _extractTextRequest.VersionId, _extractTextRequest.BlobName, _correlationId));
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            // Arrange
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

            // Act
            var response = await _extractText.Run(_httpRequestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            // _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequestDto>(_serializedExtractTextRequest))
            //     .Throws(exception);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);

            var response = await _extractText.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }
    }
}

