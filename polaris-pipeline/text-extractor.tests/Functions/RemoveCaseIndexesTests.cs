using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Common.Exceptions;
using Common.Dto.Request;
using Common.Handlers;
using Common.Dto.Response;
using text_extractor.Services.CaseSearchService;
using Common.Telemetry;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using Xunit;

namespace text_extractor.tests.Functions
{
    public class RemoveCaseIndexesTests
    {
        private readonly Fixture _fixture;
        private readonly string _serializedExtractTextRequest;
        private readonly HttpRequestMessage _httpRequestMessage;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
        private readonly Mock<ILogger<RemoveCaseIndexes>> _mockLogger;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Guid _correlationId;
        private readonly long _caseId;
        private readonly RemoveCaseIndexes _removeCaseIndexes;

        public RemoveCaseIndexesTests()
        {
            _fixture = new Fixture();
            _serializedExtractTextRequest = _fixture.Create<string>();
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(_serializedExtractTextRequest)
            };
            _mockSearchIndexService = new Mock<ISearchIndexService>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _correlationId = _fixture.Create<Guid>();
            _caseId = _fixture.Create<long>();
            _mockLogger = new Mock<ILogger<RemoveCaseIndexes>>();

            _mockSearchIndexService
                .Setup(service => service.RemoveCaseIndexEntriesAsync(It.IsAny<long>()))
                .ReturnsAsync(new IndexDocumentsDeletedResult
                {
                    DocumentCount = 1,
                    SuccessCount = 1,
                    FailureCount = 0
                });

            _removeCaseIndexes = new RemoveCaseIndexes(
                                _mockLogger.Object,
                                _mockSearchIndexService.Object,
                                _mockTelemetryAugmentationWrapper.Object,
                                _mockExceptionHandler.Object,
                                _mockJsonConvertWrapper.Object);
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RemoveCaseIndexes(null, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndSearchIndexServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RemoveCaseIndexes(_mockLogger.Object, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndTelemetryAugmentationWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RemoveCaseIndexes(_mockLogger.Object, _mockSearchIndexService.Object, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndExceptionHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RemoveCaseIndexes(_mockLogger.Object, _mockSearchIndexService.Object, _mockTelemetryAugmentationWrapper.Object, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndJsonConvertWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RemoveCaseIndexes(_mockLogger.Object, _mockSearchIndexService.Object, _mockTelemetryAugmentationWrapper.Object, _mockExceptionHandler.Object, null));
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<RemoveCaseIndexes>>()))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _removeCaseIndexes.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

            var response = await _removeCaseIndexes.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

            var response = await _removeCaseIndexes.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RemoveCaseIndexesRequestDto>(It.IsAny<string>()))
                .Returns(new RemoveCaseIndexesRequestDto());
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<IndexDocumentsDeletedResult>()))
                .Returns(string.Empty);

            var response = await _removeCaseIndexes.Run(_httpRequestMessage, _caseId);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);

            var response = await _removeCaseIndexes.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }
    }
}