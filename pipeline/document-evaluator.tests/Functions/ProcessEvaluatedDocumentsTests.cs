using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using AutoFixture;
using AutoFixture.AutoMoq;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.Requests;
using Common.Exceptions.Contracts;
using Common.Handlers;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using document_evaluator.Functions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace document_evaluation.tests.Functions;

public class ProcessEvaluatedDocumentsTests
{
    private readonly Fixture _fixture;
    private readonly string _serializedDocumentsToRemoveRequest;
    private readonly HttpRequestMessage _httpRequestMessage;
    private readonly ProcessDocumentsToRemoveRequest _documentsToRemoveRequest;
    private HttpResponseMessage _errorHttpResponseMessage;
		
    private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
    private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
    private readonly Mock<IExceptionHandler> _mockExceptionHandler;
    private readonly Mock<IValidatorWrapper<ProcessDocumentsToRemoveRequest>> _mockValidatorWrapper;
    private readonly Mock<IStorageQueueService> _mockStorageQueueService;

    private readonly Mock<ILogger<ProcessEvaluatedDocuments>> _mockLogger;
    private readonly Guid _correlationId;
    
    private readonly ProcessEvaluatedDocuments _processEvaluatedDocuments;

    public ProcessEvaluatedDocumentsTests()
    {
	    _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _serializedDocumentsToRemoveRequest = _fixture.Create<string>();
        _httpRequestMessage = new HttpRequestMessage()
        {
            Content = new StringContent(_serializedDocumentsToRemoveRequest)
        };
        _documentsToRemoveRequest = _fixture.Create<ProcessDocumentsToRemoveRequest>();
			
        _mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
        _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
        _mockValidatorWrapper = new Mock<IValidatorWrapper<ProcessDocumentsToRemoveRequest>>();
        _mockExceptionHandler = new Mock<IExceptionHandler>();
        _mockStorageQueueService = new Mock<IStorageQueueService>();

        _correlationId = _fixture.Create<Guid>();

        _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
        _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ProcessDocumentsToRemoveRequest>(_serializedDocumentsToRemoveRequest))
            .Returns(_documentsToRemoveRequest);
        _mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_documentsToRemoveRequest)).Returns(new List<ValidationResult>());
        
        _mockLogger = new Mock<ILogger<ProcessEvaluatedDocuments>>();
        _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.UpdateBlobStorageQueueName]).Returns($"update-blob-storage");
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.UpdateSearchIndexByVersionQueueName]).Returns($"update-search-index-by-version");

        _processEvaluatedDocuments = new ProcessEvaluatedDocuments(
            _mockAuthorizationValidator.Object,
            _mockLogger.Object, 
            _mockJsonConvertWrapper.Object,
            mockConfiguration.Object,
            _mockValidatorWrapper.Object,
            _mockStorageQueueService.Object);
    }
    
    [Fact]
	public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
	{
		_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<ProcessEvaluatedDocuments>>()))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Content = new StringContent(" ");
		
		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
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

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}

	[Fact]
	public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
    {
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Content = new StringContent(" ");
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}
	
	[Fact]
	public async Task Run_ReturnsBadRequestWhenContentIsNull()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Content = null;
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}
	
	[Fact]
	public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}
	
	[Fact]
	public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}

	[Fact]
	public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
	{
		var validationResults = _fixture.CreateMany<ValidationResult>(2).ToList();
		
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_documentsToRemoveRequest)).Returns(validationResults);
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}
	
	[Fact]
	public async Task Run_ReturnsOk()
	{
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(_documentsToRemoveRequest.DocumentsToRemove.Count));
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}

	[Fact]
	public async Task Run_ReturnsResponseWhenExceptionOccurs()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
		var exception = new Exception();
		_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ProcessDocumentsToRemoveRequest>(_serializedDocumentsToRemoveRequest))
			.Throws(exception);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);

		var response = await _processEvaluatedDocuments.Run(_httpRequestMessage);

		using (new AssertionScope())
		{
			_mockStorageQueueService.Verify(x => x.AddNewMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}
}
