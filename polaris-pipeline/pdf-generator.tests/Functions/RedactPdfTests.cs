using AutoFixture;
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
using System.Threading;
using Common.Domain.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Common.Wrappers.Contracts;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Handlers.Contracts;

namespace pdf_generator.tests.Functions
{
    public class RedactPdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedRedactPdfResponse;
        private HttpRequestMessage _httpRequestMessage;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<RedactPdf>> _loggerMock;
        private readonly Guid _correlationId;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<IValidator<RedactPdfRequestDto>> _mockValidator;

        private readonly RedactPdf _redactPdf;

        public RedactPdfTests()
        {
            var request = _fixture.Create<RedactPdfRequestDto>();

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };
            _correlationId = Guid.NewGuid();
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            var redactPdfResponse = _fixture.Create<RedactPdfResponse>();
            _serializedRedactPdfResponse = redactPdfResponse.ToJson();

            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentRedactionService = new Mock<IDocumentRedactionService>();

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequestDto>(It.IsAny<string>())).Returns(request);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Returns(_serializedRedactPdfResponse);

            _serializedRedactPdfResponse = redactPdfResponse.ToJson();
            mockDocumentRedactionService.Setup(x => x.RedactPdfAsync(It.IsAny<RedactPdfRequestDto>(), It.IsAny<Guid>())).ReturnsAsync(redactPdfResponse);

            _loggerMock = new Mock<ILogger<RedactPdf>>();
            _correlationId = _fixture.Create<Guid>();

            _mockValidator = new Mock<IValidator<RedactPdfRequestDto>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequestDto>(),
		            It.IsAny<CancellationToken>()))
	            .ReturnsAsync(new ValidationResult());

            _redactPdf = new RedactPdf(_mockExceptionHandler.Object, _mockJsonConvertWrapper.Object, mockDocumentRedactionService.Object, _loggerMock.Object, 
	            _mockValidator.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler
                .Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.Should().Be(errorHttpResponseMessage);
        }
        
        [Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsNull()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler
                .Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
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
			_mockExceptionHandler
                .Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
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
			_mockExceptionHandler
                .Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
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
			_mockExceptionHandler
                .Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
				.Returns(_errorHttpResponseMessage);
			_mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequestDto>(),
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
            _mockJsonConvertWrapper
                .Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Throws(exception);
            _mockExceptionHandler
                .Setup(handler => handler.HandleException(exception, It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
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
            _mockExceptionHandler
                .Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var request = _fixture.Create<RedactPdfRequestDto>();
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