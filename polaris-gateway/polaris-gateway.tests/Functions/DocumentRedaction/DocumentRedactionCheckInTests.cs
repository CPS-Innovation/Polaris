/*
using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Clients.DocumentRedaction;
using PolarisGateway.Domain.DocumentRedaction;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Functions.DocumentRedaction;
using Xunit;

namespace PolarisGateway.Tests.Functions.DocumentRedaction
{
    public class DocumentRedactionCheckInTests : SharedMethods.SharedMethods
    {
        private readonly string _caseId;
        private readonly string _documentId;

        private readonly Mock<IDocumentRedactionClient> _mockDocumentRedactionClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly DocumentRedactionCheckInDocument _documentRedactionCheckInDocument;

        public DocumentRedactionCheckInTests()
        {
            var fixture = new Fixture();
            _caseId = fixture.Create<int>().ToString();
            _documentId = fixture.Create<string>();

            _mockDocumentRedactionClient = new Mock<IDocumentRedactionClient>();
            var mockLogger = new Mock<ILogger<DocumentRedactionCheckInDocument>>();
            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _mockDocumentRedactionClient
                .Setup(s => s.CheckInDocumentAsync(_caseId, _documentId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(DocumentRedactionStatus.CheckedIn);

            _documentRedactionCheckInDocument = new DocumentRedactionCheckInDocument(mockLogger.Object, _mockDocumentRedactionClient.Object, _mockTokenValidator.Object);
        }
        
        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequestWithoutCorrelationId(), _caseId, _documentId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequestWithoutToken(), _caseId, _documentId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }
        
        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequest(), _caseId, _documentId);

            response.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Theory]
        [InlineData("x", "12345")]
        [InlineData("", "12345")]
        [InlineData("5.54322", "12345")]
        public async Task Run_ReturnsBadRequestWhenCaseIdIsNotANumber(string caseId, string documentId)
        {
            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequest(), caseId, documentId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("1", null)]
        [InlineData("1", "")]
        [InlineData("1", " ")]
        public async Task Run_ReturnsBadRequestWhenDocumentIdIsMissing(string caseId, string documentId)
        {
            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequest(), caseId, documentId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequest(), _caseId, _documentId);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockDocumentRedactionClient.Setup(client => client.CheckOutDocumentAsync(_caseId, _documentId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _documentRedactionCheckInDocument.Run(CreateHttpRequest(), _caseId, _documentId) as StatusCodeResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}
*/
