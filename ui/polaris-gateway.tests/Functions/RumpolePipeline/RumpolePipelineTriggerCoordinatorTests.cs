using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Moq;
using RumpoleGateway.Clients.OnBehalfOfTokenClient;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Domain.RumpolePipeline;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Factories;
using RumpoleGateway.Functions.RumpolePipeline;
using Xunit;

namespace RumpoleGateway.Tests.Functions.RumpolePipeline
{
	public class RumpolePipelineTriggerCoordinatorTests : SharedMethods.SharedMethods
	{
        private readonly HttpRequest _request;
        private readonly string _caseUrn;
		private readonly int _caseId;
		private readonly string _onBehalfOfAccessToken;
		private readonly string _upstreamToken;
		private readonly string _rumpolePipelineCoordinatorScope;
		private readonly TriggerCoordinatorResponse _triggerCoordinatorResponse;

        private readonly Mock<IOnBehalfOfTokenClient> _mockOnBehalfOfTokenClient;
		private readonly Mock<IPipelineClient> _mockPipelineClient;
		private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly RumpolePipelineTriggerCoordinator _rumpolePipelineTriggerCoordinator;

		public RumpolePipelineTriggerCoordinatorTests()
		{
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
			_caseId = fixture.Create<int>();
			_onBehalfOfAccessToken = fixture.Create<string>();
			_rumpolePipelineCoordinatorScope = fixture.Create<string>();
			_upstreamToken = "sample-token";
			_request = CreateHttpRequest();
			_triggerCoordinatorResponse = fixture.Create<TriggerCoordinatorResponse>();

			var mockLogger = new Mock<ILogger<RumpolePipelineTriggerCoordinator>>();
			_mockOnBehalfOfTokenClient = new Mock<IOnBehalfOfTokenClient>();
			_mockPipelineClient = new Mock<IPipelineClient>();
			var mockConfiguration = new Mock<IConfiguration>();
			var mockTriggerCoordinatorResponseFactory = new Mock<ITriggerCoordinatorResponseFactory>();

			_mockOnBehalfOfTokenClient.Setup(client => client.GetAccessTokenAsync(It.IsAny<string>(), _rumpolePipelineCoordinatorScope, It.IsAny<Guid>()))
				.ReturnsAsync(_onBehalfOfAccessToken);
			mockConfiguration.Setup(config => config[ConfigurationKeys.PipelineCoordinatorScope]).Returns(_rumpolePipelineCoordinatorScope);
			mockTriggerCoordinatorResponseFactory.Setup(factory => factory.Create(_request, It.IsAny<Guid>())).Returns(_triggerCoordinatorResponse);

            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _rumpolePipelineTriggerCoordinator =
				new RumpolePipelineTriggerCoordinator(mockLogger.Object, _mockOnBehalfOfTokenClient.Object, _mockPipelineClient.Object, mockConfiguration.Object, mockTriggerCoordinatorResponseFactory.Object, _mockTokenValidator.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
		{
			var response = await _rumpolePipelineTriggerCoordinator.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
		{
			var response = await _rumpolePipelineTriggerCoordinator.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenUpstreamTokenIsMissing()
		{
			var response = await _rumpolePipelineTriggerCoordinator.Run(CreateHttpRequestWithoutUpstreamToken(), _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
		{
			_mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
			var response = await _rumpolePipelineTriggerCoordinator.Run(CreateHttpRequest(), _caseUrn, _caseId);

			response.Should().BeOfType<UnauthorizedObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenCaseUrnIsNotSupplied()
		{
			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, string.Empty, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenForceIsNotABool()
		{
			_request.Query = new QueryCollection(new Dictionary<string, StringValues> { { "force", new StringValues("not a bool") } });
			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_TriggersCoordinator()
        {
			await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

			_mockPipelineClient.Verify(client => client.TriggerCoordinatorAsync(_caseUrn, _caseId, _onBehalfOfAccessToken, _upstreamToken, false, It.IsAny<Guid>()));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

			response.Should().BeOfType<OkObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsTriggerCoordinatorResponse()
		{
			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as OkObjectResult;

			response?.Value.Should().Be(_triggerCoordinatorResponse);
		
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenMsalExceptionOccurs()
		{
			_mockOnBehalfOfTokenClient.Setup(client => client.GetAccessTokenAsync(It.IsAny<string>(), _rumpolePipelineCoordinatorScope, It.IsAny<Guid>()))
				.ThrowsAsync(new MsalException());

			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
		{
			_mockPipelineClient.Setup(client => client.TriggerCoordinatorAsync(_caseUrn, _caseId, _onBehalfOfAccessToken, _upstreamToken, false, It.IsAny<Guid>()))
				.ThrowsAsync(new HttpRequestException());

			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
		{
			_mockPipelineClient.Setup(client => client.TriggerCoordinatorAsync(_caseUrn, _caseId, _onBehalfOfAccessToken, _upstreamToken, false, It.IsAny<Guid>()))
				.ThrowsAsync(new Exception());

			var response = await _rumpolePipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

			response?.StatusCode.Should().Be(500);
		}
	}
}

