using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Functions.RumpolePipeline;
using Xunit;

namespace RumpoleGateway.Tests.Functions.RumpolePipeline
{
	public class RumpolePipelineGetPdfTests : SharedMethods.SharedMethods
	{
        private readonly string _blobName;
		private readonly Stream _blobStream;

		private readonly Mock<IBlobStorageClient> _mockBlobStorageClient;
		private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly RumpolePipelineGetPdf _rumpolePipelineGetPdf;

		public RumpolePipelineGetPdfTests()
		{
            var fixture = new Fixture();
			_blobName = fixture.Create<string>();
			_blobStream = new MemoryStream();

			_mockBlobStorageClient = new Mock<IBlobStorageClient>();
            var mockLogger = new Mock<ILogger<RumpolePipelineGetPdf>>();
            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _mockBlobStorageClient.Setup(client => client.GetDocumentAsync(_blobName, It.IsAny<Guid>())).ReturnsAsync(_blobStream);

			_rumpolePipelineGetPdf = new RumpolePipelineGetPdf(_mockBlobStorageClient.Object, mockLogger.Object, _mockTokenValidator.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
		{
			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequestWithoutCorrelationId(), _blobName);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
		{
			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequestWithoutToken(), _blobName);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
	        _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), _blobName);

			response.Should().BeOfType<UnauthorizedObjectResult>();
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task Run_ReturnsBadRequestWhenBlobNameIsInvalid(string blobName)
		{
			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), blobName);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsNotFoundWhenBlobStorageClientReturnsNull()
		{
			_mockBlobStorageClient.Setup(client => client.GetDocumentAsync(_blobName, It.IsAny<Guid>())).ReturnsAsync(default(Stream));

			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), _blobName);

			response.Should().BeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), _blobName);

			response.Should().BeOfType<OkObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsBlobStream()
		{
			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), _blobName) as OkObjectResult;

			response?.Value.Should().Be(_blobStream);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenRequestFailedExceptionOccurs()
		{
			_mockBlobStorageClient.Setup(client => client.GetDocumentAsync(_blobName, It.IsAny<Guid>()))
				.ThrowsAsync(new RequestFailedException(500, "Test request failed exception"));

			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), _blobName) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
		{
			_mockBlobStorageClient.Setup(client => client.GetDocumentAsync(_blobName, It.IsAny<Guid>()))
				.ThrowsAsync(new Exception());

			var response = await _rumpolePipelineGetPdf.Run(CreateHttpRequest(), _blobName) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}
	}
}

