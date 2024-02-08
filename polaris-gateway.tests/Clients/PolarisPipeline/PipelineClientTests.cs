using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;
using FluentAssertions;
using Gateway.Clients.PolarisPipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Gateway.Clients.PolarisPipeline.Contracts;
using Xunit;
using Common.Dto.Tracker;
using Common.Streaming;

namespace PolarisGateway.Tests.Clients.PolarisPipeline
{
    public class PipelineClientTests
    {
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly string _cmsAuthValues;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpResponseMessage _getTrackerHttpResponseMessage;
        private readonly string _polarisPipelineFunctionAppKey;
        private readonly TrackerDto _tracker;
        private readonly Guid _correlationId;
        private readonly Mock<IPipelineClientRequestFactory> _mockRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly Mock<IHttpResponseMessageStreamFactory> _mockHttpResponseMessageStreamFactory;
        private readonly IPipelineClient _triggerCoordinatorPipelineClient;
        private readonly IPipelineClient _getTrackerPipelineClient;

        public PipelineClientTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _cmsAuthValues = "sample-token";
            _httpRequestMessage = new HttpRequestMessage();
            _tracker = fixture.Create<TrackerDto>();
            _correlationId = fixture.Create<Guid>();
            var triggerCoordinatorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            _getTrackerHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(_tracker))
            };
            _polarisPipelineFunctionAppKey = fixture.Create<string>();

            // https://carlpaton.github.io/2021/01/mocking-httpclient-sendasync/
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            httpMessageHandlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(nameof(HttpClient.SendAsync), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(response);

            _httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://base.url/") };


            var mockTrackerHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockTrackerHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_getTrackerHttpResponseMessage);
            var getTrackerHttpClient = new HttpClient(mockTrackerHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _mockRequestFactory = new Mock<IPipelineClientRequestFactory>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

            var mockPipelineClientLogger = new Mock<ILogger<PipelineClient>>();

            mockConfiguration.Setup(config => config[PipelineSettings.PipelineCoordinatorFunctionAppKey]).Returns(_polarisPipelineFunctionAppKey);

            _mockRequestFactory
                .Setup(factory => factory.Create(HttpMethod.Post, $"urns/{_caseUrn}/cases/{_caseId}?code={_polarisPipelineFunctionAppKey}", It.IsAny<Guid>(), _cmsAuthValues))
                .Returns(_httpRequestMessage);
            _mockRequestFactory
                .Setup(factory => factory.Create(HttpMethod.Get, $"urns/{_caseUrn}/cases/{_caseId}/tracker?code={_polarisPipelineFunctionAppKey}", It.IsAny<Guid>(), null))
                .Returns(_httpRequestMessage);

            var stringContent = _getTrackerHttpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<TrackerDto>(stringContent, It.IsAny<Guid>())).Returns(_tracker);

            _mockHttpResponseMessageStreamFactory = new Mock<IHttpResponseMessageStreamFactory>();

            _triggerCoordinatorPipelineClient = new PipelineClient(_mockRequestFactory.Object,
                                                                   _httpClient,
                                                                   mockConfiguration.Object,
                                                                   mockJsonConvertWrapper.Object,
                                                                   _mockHttpResponseMessageStreamFactory.Object,
                                                                   mockPipelineClientLogger.Object);
            _getTrackerPipelineClient = new PipelineClient(_mockRequestFactory.Object,
                                                           _httpClient,
                                                           mockConfiguration.Object,
                                                           mockJsonConvertWrapper.Object,
                                                           _mockHttpResponseMessageStreamFactory.Object,
                                                           mockPipelineClientLogger.Object);
        }

        [Fact]
        public async Task TriggerCoordinator_UrlIsGenerated()
        {
            await _triggerCoordinatorPipelineClient.RefreshCaseAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);

            _mockRequestFactory.Verify(factory => factory.Create(HttpMethod.Post, $"urns/{_caseUrn}/cases/{_caseId}?code={_polarisPipelineFunctionAppKey}", _correlationId, _cmsAuthValues));
        }

        [Fact]
        public async Task TriggerCoordinator_TriggersCoordinatorSuccessfully()
        {
            await _triggerCoordinatorPipelineClient.RefreshCaseAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);
        }

        [Fact(Skip = "Overly complex to mock HTTP requests via HTTP Client Factory")]
        public async Task GetTracker_ReturnsTracker()
        {
            var response = await _getTrackerPipelineClient.GetTrackerAsync(_caseUrn, _caseId, _correlationId);

            response.Should().Be(_tracker);
        }

        [Fact]
        public async Task GetTracker_ReturnsNullWhenNotFoundStatusCodeReturned()
        {
            _getTrackerHttpResponseMessage.StatusCode = HttpStatusCode.NotFound;

            var response = await _getTrackerPipelineClient.GetTrackerAsync(_caseUrn, _caseId, _correlationId);

            response.Should().BeNull();
        }
    }
}

