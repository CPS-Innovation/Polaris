using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.Exceptions;
using Common.Dto.Request;
using Common.Handlers.Contracts;
using Common.Services.CaseSearchService;
using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using Xunit;

namespace text_extractor.tests.Functions
{
    public class WaitForCaseEmptyResultsTests
    {
        private readonly Fixture _fixture;
        private readonly string _serializedExtractTextRequest;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly WaitForCaseEmptyResultsRequestDto _waitForCaseEmptyResultsRequestDto;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
        private readonly Mock<ILogger<WaitForCaseEmptyResults>> _mockLogger;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Guid _correlationId;
        private readonly WaitForCaseEmptyResults _waitForCaseEmptyResults;

        public WaitForCaseEmptyResultsTests()
        {
            _fixture = new Fixture();
            _serializedExtractTextRequest = _fixture.Create<string>();
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(_serializedExtractTextRequest)
            };
            _waitForCaseEmptyResultsRequestDto = _fixture.Create<WaitForCaseEmptyResultsRequestDto>();
            _mockSearchIndexService = new Mock<ISearchIndexService>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _correlationId = _fixture.Create<Guid>();
            _mockLogger = new Mock<ILogger<WaitForCaseEmptyResults>>();

            _mockSearchIndexService
                .Setup(service => service.WaitForCaseEmptyResultsAsync(It.IsAny<long>()))
                .ReturnsAsync(new IndexSettledResult
                {
                    TargetCount = 1,
                    RecordCounts = new List<long> { 1 },
                    IsSuccess = true
                });

            _waitForCaseEmptyResults = new WaitForCaseEmptyResults(
                                _mockLogger.Object,
                                _mockSearchIndexService.Object,
                                _mockJsonConvertWrapper.Object,
                                _mockTelemetryAugmentationWrapper.Object,
                                _mockExceptionHandler.Object);
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitForCaseEmptyResults(null, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndSearchIndexServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitForCaseEmptyResults(_mockLogger.Object, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndJsonConvertWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitForCaseEmptyResults(_mockLogger.Object, _mockSearchIndexService.Object, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndTelemetryAugmentationWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitForCaseEmptyResults(_mockLogger.Object, _mockSearchIndexService.Object, _mockJsonConvertWrapper.Object, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndExceptionHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitForCaseEmptyResults(_mockLogger.Object, _mockSearchIndexService.Object, _mockJsonConvertWrapper.Object, _mockTelemetryAugmentationWrapper.Object, null));
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<WaitForCaseEmptyResults>>()))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _waitForCaseEmptyResults.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

            var response = await _waitForCaseEmptyResults.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

            var response = await _waitForCaseEmptyResults.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<WaitForCaseEmptyResultsRequestDto>(It.IsAny<string>()))
                .Returns(new WaitForCaseEmptyResultsRequestDto());
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<IndexSettledResult>()))
                .Returns(string.Empty);

            var response = await _waitForCaseEmptyResults.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);

            var response = await _waitForCaseEmptyResults.Run(_httpRequestMessage);

            response.Should().Be(_errorHttpResponseMessage);
        }
    }
}