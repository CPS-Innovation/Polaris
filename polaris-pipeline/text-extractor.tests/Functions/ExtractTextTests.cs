﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Common.Domain.Exceptions;
using Common.Domain.Requests;
using Common.Exceptions.Contracts;
using Common.Handlers;
using Common.Services.SearchIndexService.Contracts;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using text_extractor.Services.OcrService;
using Xunit;

namespace text_extractor.tests.Functions
{
	public class ExtractTextTests
	{
		private readonly Fixture _fixture;
        private readonly string _serializedExtractTextRequest;
		private readonly HttpRequestMessage _httpRequestMessage;
		private readonly ExtractTextRequest _extractTextRequest;
		private HttpResponseMessage _errorHttpResponseMessage;
		
		private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
		private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
		private readonly Mock<IExceptionHandler> _mockExceptionHandler;
		private readonly Mock<AnalyzeResults> _mockAnalyzeResults;
		private readonly Mock<IValidatorWrapper<ExtractTextRequest>> _mockValidatorWrapper;

		private readonly Mock<ILogger<ExtractText>> _mockLogger;
		private readonly Guid _correlationId;

		private readonly ExtractText _extractText;

		public ExtractTextTests()
		{
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

			_serializedExtractTextRequest = _fixture.Create<string>();
			_httpRequestMessage = new HttpRequestMessage()
			{
				Content = new StringContent(_serializedExtractTextRequest)
			};
			_extractTextRequest = _fixture.Create<ExtractTextRequest>();
			
			_mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockValidatorWrapper = new Mock<IValidatorWrapper<ExtractTextRequest>>();
			var mockOcrService = new Mock<IOcrService>();
			_mockSearchIndexService = new Mock<ISearchIndexService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();
			_mockAnalyzeResults = new Mock<AnalyzeResults>();

			_correlationId = _fixture.Create<Guid>();

			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Returns(_extractTextRequest);
			_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_extractTextRequest)).Returns(new List<ValidationResult>());
			mockOcrService.Setup(service => service.GetOcrResultsAsync(_extractTextRequest.BlobName, It.IsAny<Guid>()))
				.ReturnsAsync(_mockAnalyzeResults.Object);

			_mockLogger = new Mock<ILogger<ExtractText>>();

			_extractText = new ExtractText(
								_mockAuthorizationValidator.Object,
								_mockJsonConvertWrapper.Object,
								_mockValidatorWrapper.Object,
								mockOcrService.Object,
								_mockSearchIndexService.Object,
								_mockExceptionHandler.Object,
								_mockLogger.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
		{
			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<ExtractText>>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			
			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
		{
			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsNull()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = null;
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

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
		public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
		{
			var validationResults = _fixture.CreateMany<ValidationResult>(2).ToList();
			
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_extractTextRequest)).Returns(validationResults);
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_StoresOcrResults()
		{
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			await _extractText.Run(_httpRequestMessage);

			_mockSearchIndexService.Verify(service => service.StoreResultsAsync(_mockAnalyzeResults.Object, _extractTextRequest.CaseId, _extractTextRequest.DocumentId,
				_extractTextRequest.VersionId, _extractTextRequest.BlobName, _correlationId));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			var response = await _extractText.Run(_httpRequestMessage);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsResponseWhenExceptionOccurs()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			var exception = new Exception();
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Throws(exception);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

