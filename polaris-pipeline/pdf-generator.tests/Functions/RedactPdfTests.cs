﻿using AutoFixture;
using Common.Wrappers;
using Moq;
using pdf_generator.Functions;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Domain.Exceptions;
using FluentAssertions;
using Newtonsoft.Json;
using pdf_generator.Services.DocumentRedactionService;
using Xunit;
using System;
using System.Net.Http.Headers;
using System.Threading;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Exceptions.Contracts;
using Common.Handlers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace pdf_generator.tests.Functions
{
    public class RedactPdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedRedactPdfResponse;
        private HttpRequestMessage _httpRequestMessage;
        private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<RedactPdf>> _loggerMock;
        private readonly Guid _correlationId;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<IValidator<RedactPdfRequest>> _mockValidator;

        private readonly RedactPdf _redactPdf;

        public RedactPdfTests()
        {
            var request = _fixture.Create<RedactPdfRequest>();

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };
            _correlationId = Guid.NewGuid();
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            var redactPdfResponse = _fixture.Create<RedactPdfResponse>();
            _serializedRedactPdfResponse = redactPdfResponse.ToJson();

            _mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentRedactionService = new Mock<IDocumentRedactionService>();

            _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequest>(It.IsAny<string>())).Returns(request);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Returns(_serializedRedactPdfResponse);

            _serializedRedactPdfResponse = redactPdfResponse.ToJson();
            mockDocumentRedactionService.Setup(x => x.RedactPdfAsync(It.IsAny<RedactPdfRequest>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(redactPdfResponse);

            _loggerMock = new Mock<ILogger<RedactPdf>>();
            _correlationId = _fixture.Create<Guid>();

            _mockValidator = new Mock<IValidator<RedactPdfRequest>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequest>(),
		            It.IsAny<CancellationToken>()))
	            .ReturnsAsync(new ValidationResult());

            _redactPdf = new RedactPdf(_mockAuthorizationValidator.Object, _mockExceptionHandler.Object,
                _mockJsonConvertWrapper.Object, mockDocumentRedactionService.Object, _loggerMock.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
        {
            _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.Should().Be(errorHttpResponseMessage);
        }
        
        [Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsNull()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = null;
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _redactPdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Headers.Clear();
			_httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

			var response = await _redactPdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Headers.Clear();
			_httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

			var response = await _redactPdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
		{
			var testFailures = _fixture.CreateMany<ValidationFailure>(2);
			
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
				.Returns(_errorHttpResponseMessage);
			_mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequest>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ValidationResult(testFailures));
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _redactPdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
	        _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
	        
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Throws(exception);
            _mockExceptionHandler.Setup(handler => handler.HandleException(exception, It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
	        var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
			var response = await _redactPdf.Run(_httpRequestMessage);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be(_serializedRedactPdfResponse);
        }

        [Fact]
        public async Task Run_ReturnsBadRequest_WhenValidationFailed()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var request = _fixture.Create<RedactPdfRequest>();
            request.CaseId = 0;

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
