﻿using System;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Functions.RumpolePipeline;
using RumpoleGateway.Services;
using Xunit;

namespace RumpoleGateway.Tests.Functions.RumpolePipeline
{
	public class RumpolePipelineGetSasUrlTests : SharedMethods.SharedMethods
	{
        private readonly string _blobName;
        private readonly string _fakeSasUrl;
		
		private readonly Mock<ISasGeneratorService> _mockSasGeneratorService;
		private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly RumpolePipelineGetSasUrl _rumpolePipelineGetSasUrl;

		public RumpolePipelineGetSasUrlTests()
		{
            var fixture = new Fixture();
			_blobName = fixture.Create<string>();
            _fakeSasUrl = fixture.Create<string>();

			_mockSasGeneratorService = new Mock<ISasGeneratorService>();
            _mockTokenValidator = new Mock<IAuthorizationValidator>();
			var mockLogger = new Mock<ILogger<RumpolePipelineGetSasUrl>>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockSasGeneratorService.Setup(client => client.GenerateSasUrlAsync(_blobName, It.IsAny<Guid>())).ReturnsAsync(_fakeSasUrl);

            _rumpolePipelineGetSasUrl = new RumpolePipelineGetSasUrl(_mockTokenValidator.Object, mockLogger.Object, _mockSasGeneratorService.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
		{
			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequestWithoutCorrelationId(), _blobName);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
		{
			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequestWithoutToken(), _blobName);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
	        _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), _blobName);

			response.Should().BeOfType<UnauthorizedObjectResult>();
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task Run_ReturnsBadRequestWhenBlobNameIsInvalid(string blobName)
		{
			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), blobName);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsNotFoundWhenSasUrlGeneratorReturnsNull()
		{
			_mockSasGeneratorService.Setup(client => client.GenerateSasUrlAsync(_blobName, It.IsAny<Guid>())).ReturnsAsync((string)null);

			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), _blobName);

			response.Should().BeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), _blobName);

			response.Should().BeOfType<OkObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsGeneratedUrl()
		{
			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), _blobName) as OkObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response?.Value.Should().Be(_fakeSasUrl);
            }
        }

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenRequestFailedExceptionOccurs()
		{
            _mockSasGeneratorService.Setup(client => client.GenerateSasUrlAsync(_blobName, It.IsAny<Guid>()))
				.ThrowsAsync(new RequestFailedException(500, "Test request failed exception"));

			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), _blobName) as ObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response?.StatusCode.Should().Be(500);
            }
        }

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
		{
			_mockSasGeneratorService.Setup(client => client.GenerateSasUrlAsync(_blobName, It.IsAny<Guid>()))
				.ThrowsAsync(new Exception());

			var response = await _rumpolePipelineGetSasUrl.Run(CreateHttpRequest(), _blobName) as ObjectResult;

            using (new AssertionScope())
            {
                response.Should().NotBeNull();
                response?.StatusCode.Should().Be(500);
            }
        }
	}
}

