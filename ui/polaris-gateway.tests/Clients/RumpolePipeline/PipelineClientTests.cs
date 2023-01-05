using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Domain.RumpolePipeline;
using RumpoleGateway.Factories;
using RumpoleGateway.Wrappers;
using Xunit;

namespace RumpoleGateway.Tests.Clients.RumpolePipeline
{
	public class PipelineClientTests
	{
		private readonly string _caseUrn;
        private readonly int _caseId;
		private readonly string _accessToken;
		private readonly string _upstreamToken;
		private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpResponseMessage _getTrackerHttpResponseMessage;
		private readonly string _rumpolePipelineFunctionAppKey;
		private readonly Tracker _tracker;
		private readonly Guid _correlationId;

        private readonly Mock<IPipelineClientRequestFactory> _mockRequestFactory;

        private readonly IPipelineClient _triggerCoordinatorPipelineClient;
		private readonly IPipelineClient _getTrackerPipelineClient;

		public PipelineClientTests()
		{
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
			_caseId = fixture.Create<int>();
			_accessToken = fixture.Create<string>();
			_upstreamToken = "sample-token";
			_httpRequestMessage = new HttpRequestMessage();
			_tracker = fixture.Create<Tracker>();
			_correlationId = fixture.Create<Guid>();
			var triggerCoordinatorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			_getTrackerHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(JsonConvert.SerializeObject(_tracker))
			};
			_rumpolePipelineFunctionAppKey = fixture.Create<string>();

			var mockTriggerCoordinatorHttpMessageHandler = new Mock<HttpMessageHandler>();
			mockTriggerCoordinatorHttpMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(triggerCoordinatorHttpResponseMessage);
			var triggerCoordinatorHttpClient = new HttpClient(mockTriggerCoordinatorHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

			var mockTrackerHttpMessageHandler = new Mock<HttpMessageHandler>();
			mockTrackerHttpMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(_getTrackerHttpResponseMessage);
			var getTrackerHttpClient = new HttpClient(mockTrackerHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

			_mockRequestFactory = new Mock<IPipelineClientRequestFactory>();
			var mockConfiguration = new Mock<IConfiguration>();
			var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

			var mockPipelineClientLogger = new Mock<ILogger<PipelineClient>>();

			mockConfiguration.Setup(config => config[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]).Returns(_rumpolePipelineFunctionAppKey);

			_mockRequestFactory.Setup(factory => factory.CreateGet($"cases/{_caseUrn}/{_caseId}?code={_rumpolePipelineFunctionAppKey}", _accessToken, _upstreamToken, It.IsAny<Guid>())).Returns(_httpRequestMessage);
			_mockRequestFactory.Setup(factory => factory.CreateGet($"cases/{_caseUrn}/{_caseId}/tracker?code={_rumpolePipelineFunctionAppKey}", _accessToken, It.IsAny<Guid>())).Returns(_httpRequestMessage);

			var stringContent = _getTrackerHttpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<Tracker>(stringContent, It.IsAny<Guid>())).Returns(_tracker);

			_triggerCoordinatorPipelineClient = new PipelineClient(_mockRequestFactory.Object, triggerCoordinatorHttpClient, mockConfiguration.Object, mockJsonConvertWrapper.Object, mockPipelineClientLogger.Object);
			_getTrackerPipelineClient = new PipelineClient(_mockRequestFactory.Object, getTrackerHttpClient, mockConfiguration.Object, mockJsonConvertWrapper.Object, mockPipelineClientLogger.Object);
		}

		[Fact]
		public async Task TriggerCoordinator_UrlHasNoForceQueryWhenForceIsFalse()
		{
			await _triggerCoordinatorPipelineClient.TriggerCoordinatorAsync(_caseUrn, _caseId, _accessToken, _upstreamToken, false, _correlationId);

			_mockRequestFactory.Verify(factory => factory.CreateGet($"cases/{_caseUrn}/{_caseId}?code={_rumpolePipelineFunctionAppKey}", _accessToken, _upstreamToken, _correlationId));
		}

		[Fact]
		public async Task TriggerCoordinator_UrlHasForceQueryWhenForceIsTrue()
		{
			var url = $"cases/{_caseUrn}/{_caseId}?code={_rumpolePipelineFunctionAppKey}&&force=true";
			_mockRequestFactory.Setup(factory => factory.CreateGet(url, _accessToken, _upstreamToken, It.IsAny<Guid>())).Returns(_httpRequestMessage);

			await _triggerCoordinatorPipelineClient.TriggerCoordinatorAsync(_caseUrn, _caseId, _accessToken, _upstreamToken, true, _correlationId);

			_mockRequestFactory.Verify(factory => factory.CreateGet(url, _accessToken, _upstreamToken, _correlationId));
		}

		[Fact]
		public async Task TriggerCoordinator_TriggersCoordinatorSuccessfully()
        {
			await _triggerCoordinatorPipelineClient.TriggerCoordinatorAsync(_caseUrn, _caseId, _accessToken, _upstreamToken, false, _correlationId);
        }

		[Fact]
		public async Task GetTracker_ReturnsTracker()
		{
			var response = await _getTrackerPipelineClient.GetTrackerAsync(_caseUrn, _caseId, _accessToken, _correlationId);

			response.Should().Be(_tracker);
		}

		[Fact]
		public async Task GetTracker_ReturnsNullWhenNotFoundStatusCodeReturned()
		{
			_getTrackerHttpResponseMessage.StatusCode = HttpStatusCode.NotFound;

			var response = await _getTrackerPipelineClient.GetTrackerAsync(_caseUrn, _caseId, _accessToken, _correlationId);

			response.Should().BeNull();
		}
	}
}

