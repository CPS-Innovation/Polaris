using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Exceptions;
using Common.Dto.Response;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using text_extractor.Services.CaseSearchService;
using Xunit;

namespace text_extractor.tests.Functions
{
    public class CaseIndexCountTests : BaseTestClass
    {
        private readonly Fixture _fixture;
        private JsonResult _errorResult;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ILogger<CaseIndexCount>> _mockLogger;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Guid _correlationId;
        private readonly int _caseId;
        private readonly CaseIndexCount _caseIndexCount;

        public CaseIndexCountTests()
        {
            _fixture = new Fixture();

            _mockSearchIndexService = new Mock<ISearchIndexService>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _correlationId = _fixture.Create<Guid>();
            _mockLogger = new Mock<ILogger<CaseIndexCount>>();
            _caseId = _fixture.Create<int>();

            _mockSearchIndexService
                .Setup(service => service.GetCaseIndexCount(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new SearchIndexCountResult(100));

            _caseIndexCount = new CaseIndexCount(
                _mockLogger.Object,
                _mockSearchIndexService.Object,
                _mockExceptionHandler.Object
            );
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndSearchIndexServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndJsonConvertWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, _mockSearchIndexService.Object, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndTelemetryAugmentationWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, _mockSearchIndexService.Object, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndExceptionHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CaseIndexCount(_mockLogger.Object, _mockSearchIndexService.Object, null));
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), null);
            _errorResult = new JsonResult(_fixture.Create<string>()) { StatusCode = (int)HttpStatusCode.Unauthorized };
            _mockExceptionHandler.Setup(handler => handler.HandleExceptionNew(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<CaseIndexCount>>()))
                .Returns(_errorResult);
            
            var response = await _caseIndexCount.Run(mockRequest.Object, _caseId);

            response.Should().Be(_errorResult);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), Guid.Empty);
            _errorResult = new JsonResult(_fixture.Create<string>()) { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler.Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorResult);
            
            var response = await _caseIndexCount.Run(mockRequest.Object, _caseId);

            response.Should().Be(_errorResult);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), _correlationId);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<SearchIndexCountResult>()))
                .Returns(string.Empty);

            var response = await _caseIndexCount.Run(mockRequest.Object, _caseId);

            var result = response as JsonResult;
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), _correlationId);
            _errorResult = new JsonResult(_fixture.Create<string>());
            _mockExceptionHandler.Setup(handler => handler.HandleExceptionNew(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
                .Returns(_errorResult);
            _mockSearchIndexService.Setup(s => s.GetCaseIndexCount(It.IsAny<int>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test exception"));

            var response = await _caseIndexCount.Run(mockRequest.Object, _caseId);

            response.Should().Be(_errorResult);
        }
    }
}