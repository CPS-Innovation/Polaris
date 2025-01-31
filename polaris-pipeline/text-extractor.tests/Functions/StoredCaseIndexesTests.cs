using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using AutoFixture;
using AutoFixture.AutoMoq;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Exceptions;
using Common.Handlers;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using text_extractor.Functions;
using text_extractor.Mappers.Contracts;
using text_extractor.Services.CaseSearchService;
using Xunit;

namespace text_extractor.tests.Functions
{
    public class StoreCaseIndexesTests : BaseTestClass
    {
        private readonly Fixture _fixture;
        private readonly StoreCaseIndexesRequestDto _storeCaseIndexesRequest;
        private JsonResult _errorResult;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly AnalyzeResults _mockAnalyzeResults;
        private readonly Mock<ILogger<StoreCaseIndexes>> _mockLogger;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Guid _correlationId;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly long _versionId;
        private readonly string _documentId;
        private readonly StoreCaseIndexes _storeCaseIndexes;
        private readonly List<ValidationResult> _validationResults;

        public StoreCaseIndexesTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _storeCaseIndexesRequest = _fixture.Create<StoreCaseIndexesRequestDto>();
            var mockValidatorWrapper = new Mock<IValidatorWrapper<StoreCaseIndexesRequestDto>>();
            _mockSearchIndexService = new Mock<ISearchIndexService>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _mockAnalyzeResults = Mock.Of<AnalyzeResults>(ctx => ctx.ReadResults == new List<ReadResult>());
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

            _correlationId = _fixture.Create<Guid>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _versionId = _fixture.Create<long>();
            _documentId = _fixture.Create<string>();

            _validationResults = [];
            mockValidatorWrapper
                .Setup(wrapper => wrapper.Validate(_storeCaseIndexesRequest))
                .Returns(_validationResults);

            _mockLogger = new Mock<ILogger<StoreCaseIndexes>>();

            _mockJsonConvertWrapper
                .Setup(wrapper => wrapper.DeserializeObject<AnalyzeResults>(It.IsAny<string>()))
                .Returns(_mockAnalyzeResults);

            var mockDtoHttpRequestHeadersMapper = new Mock<IDtoHttpRequestHeadersMapper>();

            mockDtoHttpRequestHeadersMapper.Setup(mapper => mapper.Map<StoreCaseIndexesRequestDto>(It.IsAny<IHeaderDictionary>()))
                .Returns(_storeCaseIndexesRequest);

            _storeCaseIndexes = new StoreCaseIndexes(
                                _mockSearchIndexService.Object,
                                _mockExceptionHandler.Object,
                                _mockLogger.Object,
                                _mockJsonConvertWrapper.Object);
        }

        [Fact]
        public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), null);
            _errorResult = new JsonResult(_fixture.Create<string>()) { StatusCode = (int)HttpStatusCode.Unauthorized };
            _mockExceptionHandler.Setup(handler => handler.HandleExceptionNew(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<StoreCaseIndexes>>(), It.IsAny<object>()))
                .Returns(_errorResult);

            var response = await _storeCaseIndexes.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().Be(_errorResult);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), Guid.Empty);
            _errorResult = new JsonResult(_fixture.Create<string>()) { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler.Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object, It.IsAny<object>()))
                .Returns(_errorResult);

            _validationResults.Add(new ValidationResult("Invalid"));
            var response = await _storeCaseIndexes.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().Be(_errorResult);
        }

        [Fact]
        public async Task Run_StoresOcrResults()
        {
            var mockRequest = CreateMockRequest(new StringContent("{}"), _correlationId);
            await _storeCaseIndexes.Run(mockRequest.Object, _caseUrn, _caseId, _storeCaseIndexesRequest.DocumentId, _versionId);

            _mockSearchIndexService.Verify(service => service.SendStoreResultsAsync(_mockAnalyzeResults, _caseId, _storeCaseIndexesRequest.DocumentId, _versionId, _correlationId));
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            // Arrange
            var mockRequest = CreateMockRequest(new StringContent("{}"), _correlationId);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<StoreCaseIndexesResult>()))
                .Returns(string.Empty);

            // Act
            var response = await _storeCaseIndexes.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            // Assert
            var result = response as JsonResult;
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            // Arrange
            var mockRequest = CreateMockRequest(new StringContent("{}"), _correlationId);
            _errorResult = new JsonResult(_fixture.Create<string>()) { StatusCode = (int)HttpStatusCode.InternalServerError };
            _mockExceptionHandler.Setup(handler => handler.HandleExceptionNew(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object, It.IsAny<object>()))
                .Returns(_errorResult);
            _mockSearchIndexService.Setup(s => s.SendStoreResultsAsync(It.IsAny<AnalyzeResults>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>(), 
                    It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var response = await _storeCaseIndexes.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            // Assert
            response.Should().Be(_errorResult);
        }
    }
}

