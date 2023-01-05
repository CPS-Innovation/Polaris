/*
using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Clients.DocumentExtraction;
using PolarisGateway.Domain.DocumentExtraction;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Functions.DocumentExtraction;
using Xunit;

namespace PolarisGateway.Tests.Functions.DocumentExtraction
{
    public class DocumentExtractionGetCaseDocumentsTests : SharedMethods.SharedMethods
	{
        private readonly string _caseId;
		private readonly Case _case;

        private readonly Mock<IDocumentExtractionClient> _mockDocumentExtractionClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly DocumentExtractionGetCaseDocuments _documentExtractionGetCaseDocuments;

		public DocumentExtractionGetCaseDocumentsTests()
		{
            var fixture = new Fixture();
			_caseId = fixture.Create<int>().ToString();
			_case = fixture.Create<Case>();

			_mockDocumentExtractionClient = new Mock<IDocumentExtractionClient>();
			var mockLogger = new Mock<ILogger<DocumentExtractionGetCaseDocuments>>();
			_mockTokenValidator = new Mock<IAuthorizationValidator>();
            
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
			_mockDocumentExtractionClient.Setup(client => client.GetCaseDocumentsAsync(_caseId, It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(_case);

			_documentExtractionGetCaseDocuments = new DocumentExtractionGetCaseDocuments(_mockDocumentExtractionClient.Object, mockLogger.Object, _mockTokenValidator.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenCorrelationIdIsMissing()
		{
			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequestWithoutCorrelationId(), _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
		{
			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequestWithoutToken(), _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
		{
			_mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequest(), _caseId);

			response.Should().BeOfType<UnauthorizedObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenCaseIdIsNotAnInteger()
		{
			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequest(), "Not an integer");

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsNotFoundWhenPipelineClientReturnsNull()
		{
			_mockDocumentExtractionClient.Setup(client => client.GetCaseDocumentsAsync(_caseId, It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(default(Case));

			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequest(), _caseId);

			response.Should().BeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequest(), _caseId);

			response.Should().BeOfType<OkObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsCase()
        {
            var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequest(), _caseId) as OkObjectResult;

            response?.Value.Should().Be(_case);
        }

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
		{
			_mockDocumentExtractionClient.Setup(client => client.GetCaseDocumentsAsync(_caseId, It.IsAny<string>(), It.IsAny<Guid>()))
				.ThrowsAsync(new Exception());

			var response = await _documentExtractionGetCaseDocuments.Run(CreateHttpRequest(), _caseId) as StatusCodeResult;

            response?.StatusCode.Should().Be(500);
        }
	}
}
*/

