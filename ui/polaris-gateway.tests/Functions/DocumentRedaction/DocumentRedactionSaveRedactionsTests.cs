/*
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using RumpoleGateway.Clients.DocumentRedaction;
using RumpoleGateway.Clients.OnBehalfOfTokenClient;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Functions.DocumentRedaction;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Identity.Client;
using System.Net.Http;
using System;
using FluentAssertions.Execution;
using RumpoleGateway.Domain.DocumentRedaction;

namespace RumpoleGateway.Tests.Functions.DocumentRedaction
{
    public class DocumentRedactionSaveRedactionsTests : SharedMethods.SharedMethods
    {
        private readonly Fixture _fixture;
        private readonly string _caseId;
        private readonly string _documentId;
        private readonly string _fileName;
        private readonly string _onBehalfOfAccessToken;
        private readonly string _scope;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;
        private readonly Mock<IOnBehalfOfTokenClient> _mockOnBehalfOfTokenClient;
        private readonly Mock<IDocumentRedactionClient> _mockDocumentRedactionClient;
        private readonly DocumentRedactionSaveRequest _saveRequest;

        private readonly DocumentRedactionSaveRedactions _documentRedactionSaveRedactions;

        public DocumentRedactionSaveRedactionsTests()
        {
            _fixture = new Fixture();
            _onBehalfOfAccessToken = _fixture.Create<string>();
            _scope = _fixture.Create<string>();
            _caseId = _fixture.Create<int>().ToString();
            _documentId = _fixture.Create<string>();
            _fileName = _fixture.Create<string>();
            _saveRequest = _fixture.Create<DocumentRedactionSaveRequest>();

            _mockDocumentRedactionClient = new Mock<IDocumentRedactionClient>();
            var mockLogger = new Mock<ILogger<DocumentRedactionSaveRedactions>>();
            _mockOnBehalfOfTokenClient = new Mock<IOnBehalfOfTokenClient>();
            _mockOnBehalfOfTokenClient.Setup(client => client.GetAccessTokenAsync(It.IsAny<string>(), _scope, It.IsAny<Guid>()))
                .ReturnsAsync(_onBehalfOfAccessToken);
            _mockTokenValidator = new Mock<IAuthorizationValidator>();
            var mockConfiguration = new Mock<IConfiguration>();
            
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            mockConfiguration.Setup(config => config[ConfigurationKeys.PipelineRedactPdfScope]).Returns(_scope);

            _documentRedactionSaveRedactions = new DocumentRedactionSaveRedactions(mockLogger.Object,
                _mockOnBehalfOfTokenClient.Object, _mockDocumentRedactionClient.Object, mockConfiguration.Object,
                _mockTokenValidator.Object);
        }
        
        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequestWithoutCorrelationId(), _caseId, _documentId, _fileName);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequestWithoutToken(), _caseId, _documentId, _fileName);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_WhenValidationTokenIsInvalid_ReturnsUnauthorized()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), _caseId, _documentId, _fileName);

            response.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Run_WhenDocumentIsNotSupplied_ReturnsBadRequest()
        {
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), _caseId, string.Empty, _fileName);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_WhenCaseIdIsNotSupplied_ReturnsBadRequest()
        {
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), "Not an integer", _documentId, _fileName);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_WhenFileNameNotSupplied_ReturnsBadRequest()
        {
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), _caseId, _documentId, string.Empty);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var redactedDocumentUrl = _fixture.Create<string>();
            var validSaveResult = new DocumentRedactionSaveResult
            {
                Message = string.Empty,
                RedactedDocumentUrl = redactedDocumentUrl,
                Succeeded = true
            };
            _mockDocumentRedactionClient.Setup(x => x.SaveRedactionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DocumentRedactionSaveRequest>(), 
                It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(validSaveResult);
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(_saveRequest), _caseId, _documentId, _fileName) as OkObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                var result = response?.Value as DocumentRedactionSaveResult;
                result.Should().NotBeNull();
                result?.Succeeded.Should().BeTrue();
                result?.RedactedDocumentUrl.Should().Be(redactedDocumentUrl);
            }
        }

        [Fact]
        public async Task Run_InvalidRequest_FailsValidation_ReturnsBadRequest()
        {
            var invalidSaveRequest = _fixture.Create<DocumentRedactionSaveRequest>();
            invalidSaveRequest.Redactions = null;
            var message = _fixture.Create<string>();
            var validSaveResult = new DocumentRedactionSaveResult
            {
                Message = message,
                RedactedDocumentUrl = string.Empty,
                Succeeded = false
            };
            _mockDocumentRedactionClient.Setup(x => x.SaveRedactionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DocumentRedactionSaveRequest>(),
                It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(validSaveResult);
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(_saveRequest), _caseId, _documentId, _fileName) as BadRequestObjectResult;

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_WhenSaveFails_WhenAMessageIsGenerated_ThisIsContainedInTheBadRequest_ThatIsReturned()
        {
            var message = _fixture.Create<string>();
            var invalidSaveResult = new DocumentRedactionSaveResult
            {
                Message = message,
                RedactedDocumentUrl = string.Empty,
                Succeeded = false
            };
            _mockDocumentRedactionClient.Setup(x => x.SaveRedactionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DocumentRedactionSaveRequest>(),
                It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(invalidSaveResult);
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(_saveRequest), _caseId, _documentId, _fileName) as BadRequestObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                var result = response?.Value as string;
                result.Should().Be(message);
            }
        }

        [Fact]
        public async Task Run_WhenSaveFails_WhenAMessageIsNotGenerated_ABadRequestDefaultMessage_IsReturned()
        {
            var invalidSaveResult = new DocumentRedactionSaveResult
            {
                Message = string.Empty,
                RedactedDocumentUrl = string.Empty,
                Succeeded = false
            };
            var defaultMessage = $"The redaction request could not be processed for file name '{_fileName}'.";
            _mockDocumentRedactionClient.Setup(x => x.SaveRedactionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DocumentRedactionSaveRequest>(),
                It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(invalidSaveResult);
            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(_saveRequest), _caseId, _documentId, _fileName) as BadRequestObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                var result = response?.Value as string;
                result.Should().Be(defaultMessage);
            }
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenMsalExceptionOccurs()
        {
            _mockOnBehalfOfTokenClient.Setup(client => client.GetAccessTokenAsync(It.IsAny<string>(), _scope, It.IsAny<Guid>()))
                .ThrowsAsync(new MsalException());

            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), _caseId, _documentId, _fileName) as ObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response?.StatusCode.Should().Be(500);
            }
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
        {
            _mockDocumentRedactionClient.Setup(client => client.SaveRedactionsAsync(_caseId, _documentId, _fileName, _saveRequest, _onBehalfOfAccessToken, It.IsAny<Guid>()))
                .ThrowsAsync(new HttpRequestException());

            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), _caseId, _documentId, _fileName) as ObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response?.StatusCode.Should().Be(500);
            }
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockDocumentRedactionClient.Setup(client => client.SaveRedactionsAsync(_caseId, _documentId, _fileName, _saveRequest, _onBehalfOfAccessToken, It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _documentRedactionSaveRedactions.Run(CreateHttpRequest(), _caseId, _documentId, _fileName) as ObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response?.StatusCode.Should().Be(500);
            }
        }
    }
}
*/
