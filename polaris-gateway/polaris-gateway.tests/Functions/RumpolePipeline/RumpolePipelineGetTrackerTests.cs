using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Moq;
using PolarisGateway.Clients.OnBehalfOfTokenClient;
using PolarisGateway.Clients.PolarisPipeline;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Functions.PolarisPipeline;
using Xunit;

namespace PolarisGateway.Tests.Functions.PolarisPipeline
{
	public class PolarisPipelineGetTrackerTests : SharedMethods.SharedMethods
	{
		private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly Tracker _tracker;

        private readonly Mock<IOnBehalfOfTokenClient> _mockOnBehalfOfTokenClient;
		private readonly Mock<IPipelineClient> _mockPipelineClient;
		private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly PolarisPipelineGetTracker _polarisPipelineGetTracker;

		public PolarisPipelineGetTrackerTests()
		{
			var fixture = new Fixture();
			_caseUrn = fixture.Create<string>();
			_caseId = fixture.Create<int>();
			var onBehalfOfAccessToken = fixture.Create<string>();
			var polarisPipelineCoordinatorScope = fixture.Create<string>();
			_tracker = fixture.Create<Tracker>();
			fixture.Create<Guid>();

			var mockLogger = new Mock<ILogger<PolarisPipelineGetTracker>>();
			_mockOnBehalfOfTokenClient = new Mock<IOnBehalfOfTokenClient>();
			_mockPipelineClient = new Mock<IPipelineClient>();
			var mockConfiguration = new Mock<IConfiguration>();
            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockOnBehalfOfTokenClient.Setup(client => client.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(onBehalfOfAccessToken);
			_mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(_tracker);
			mockConfiguration.Setup(config => config[ConfigurationKeys.PipelineCoordinatorScope]).Returns(polarisPipelineCoordinatorScope);

			_polarisPipelineGetTracker = new PolarisPipelineGetTracker(mockLogger.Object, _mockOnBehalfOfTokenClient.Object, _mockPipelineClient.Object, mockConfiguration.Object, _mockTokenValidator.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
		{
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
		{
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
		{
			_mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

			response.Should().BeOfType<UnauthorizedObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenUpstreamTokenIsMissing()
		{
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutUpstreamToken(), _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		public async Task Run_ReturnsBadRequestWhenCaseUrnIsEmpty(string caseUrn)
		{
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsNotFoundWhenPipelineClientReturnsNull()
		{
			_mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(default(Tracker));

			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

			response.Should().BeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

			response.Should().BeOfType<OkObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsTracker()
		{
			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as OkObjectResult;

			response?.Value.Should().Be(_tracker);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenMsalExceptionOccurs()
        {
			_mockOnBehalfOfTokenClient.Setup(client => client.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ThrowsAsync(new MsalException());

			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
		{
			_mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ThrowsAsync(new HttpRequestException());

			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
		{
			_mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ThrowsAsync(new Exception());

			var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}
	}
}

