using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Common.Exceptions;
using Common.Dto.Response;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using text_extractor.Services.CaseSearchService;
using Xunit;

namespace text_extractor.tests.Functions
{
    public class CaseIndexCountTests
    {
        private readonly Fixture _fixture;
        private readonly HttpRequestMessage _httpRequestMessage;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ILogger<CaseIndexCount>> _mockLogger;
        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Guid _correlationId;
        private readonly long _caseId;
        private readonly CaseIndexCount _caseIndexCount;

        public CaseIndexCountTests()
        {
            _fixture = new Fixture();

            _httpRequestMessage = new HttpRequestMessage();
            _mockSearchIndexService = new Mock<ISearchIndexService>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _correlationId = _fixture.Create<Guid>();
            _mockLogger = new Mock<ILogger<CaseIndexCount>>();
            _caseId = _fixture.Create<long>();

            _mockSearchIndexService
                .Setup(service => service.GetCaseIndexCount(It.IsAny<long>()))
                .ReturnsAsync(new SearchIndexCountResult(100));

            _caseIndexCount = new CaseIndexCount(
                _mockLogger.Object,
                _mockSearchIndexService.Object,
                _mockJsonConvertWrapper.Object,
                _mockTelemetryAugmentationWrapper.Object,
                _mockExceptionHandler.Object
            );
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(null, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndSearchIndexServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndJsonConvertWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, _mockSearchIndexService.Object, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndTelemetryAugmentationWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, _mockSearchIndexService.Object, _mockJsonConvertWrapper.Object, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndExceptionHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, _mockSearchIndexService.Object, _mockJsonConvertWrapper.Object, _mockTelemetryAugmentationWrapper.Object, null));
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<CaseIndexCount>>()))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _caseIndexCount.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

            var response = await _caseIndexCount.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);
            _httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

            var response = await _caseIndexCount.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<SearchIndexCountResult>()))
                .Returns(string.Empty);

            var response = await _caseIndexCount.Run(_httpRequestMessage, _caseId);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorHttpResponseMessage);

            var response = await _caseIndexCount.Run(_httpRequestMessage, _caseId);

            response.Should().Be(_errorHttpResponseMessage);
        }
    }
}