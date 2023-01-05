using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using RumpoleGateway.Factories;
using RumpoleGateway.Wrappers;
using System.Net.Http;
using System.Threading.Tasks;
using RumpoleGateway.Clients.RumpolePipeline;
using Xunit;
using Moq.Protected;
using System.Threading;
using System;
using Newtonsoft.Json;
using System.Net;
using RumpoleGateway.Domain.DocumentRedaction;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace RumpoleGateway.Tests.Clients.RumpolePipeline
{
    public class RedactionClientTests
    {
        private readonly RedactPdfRequest _request;
        private readonly Mock<IPipelineClientRequestFactory> _mockRequestFactory;
        private readonly string _rumpolePipelineRedactPdfFunctionAppKey;
        private readonly Fixture _fixture;
        private readonly Guid _correlationId;
        
        private readonly IRedactionClient _redactionClient;

        public RedactionClientTests()
        {
            _fixture = new Fixture();

            _request = _fixture.Create<RedactPdfRequest>();
            _mockRequestFactory = new Mock<IPipelineClientRequestFactory>();
            _correlationId = _fixture.Create<Guid>();

            _rumpolePipelineRedactPdfFunctionAppKey = _fixture.Create<string>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

            mockConfiguration.Setup(config => config[ConfigurationKeys.PipelineRedactPdfFunctionAppKey]).Returns(_rumpolePipelineRedactPdfFunctionAppKey);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Put
            };

            _mockRequestFactory.Setup(factory => factory.CreatePut($"redactPdf?code={_rumpolePipelineRedactPdfFunctionAppKey}", It.IsAny<string>(), It.IsAny<Guid>())).Returns(httpRequestMessage);

            var redactPdfResponse = _fixture.Create<RedactPdfResponse>();
            var redactPdfResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(redactPdfResponse))
            };

            var stringContent = redactPdfResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfResponse>(stringContent, It.IsAny<Guid>())).Returns(redactPdfResponse);
            mockJsonConvertWrapper.Setup(x => x.SerializeObject(It.IsAny<RedactPdfRequest>(), It.IsAny<Guid>())).Returns(JsonConvert.SerializeObject(_request));

            var mockRedactionClientLogger = new Mock<ILogger<RedactionClient>>();
            
            var mockRedactPdfMessageHandler = new Mock<HttpMessageHandler>();
            mockRedactPdfMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(redactPdfResponseMessage);
            var redactPdfHttpClient = new HttpClient(mockRedactPdfMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _redactionClient = new RedactionClient(_mockRequestFactory.Object, redactPdfHttpClient, mockConfiguration.Object, mockJsonConvertWrapper.Object, mockRedactionClientLogger.Object);
        }

        [Fact]
        public async Task RedactPdf_CreatesTheRequestCorrectly()
        {
            var accessToken = _fixture.Create<string>();
            
            await _redactionClient.RedactPdfAsync(_request, accessToken, _correlationId);

            _mockRequestFactory.Verify(factory => factory.CreatePut($"redactPdf?code={_rumpolePipelineRedactPdfFunctionAppKey}", accessToken, _correlationId));
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrown_IsCaughtAsException()
        {
            var accessToken = _fixture.Create<string>();

            _mockRequestFactory.Setup(factory => factory.CreatePut($"redactPdf?code={_rumpolePipelineRedactPdfFunctionAppKey}", It.IsAny<string>(), It.IsAny<Guid>())).Throws<Exception>();

            var results = async() => await _redactionClient.RedactPdfAsync(_request, accessToken, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsNotFound_ReturnsNullResponse()
        {
            var accessToken = _fixture.Create<string>();

            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound);
            _mockRequestFactory.Setup(factory => factory.CreatePut($"redactPdf?code={_rumpolePipelineRedactPdfFunctionAppKey}", It.IsAny<string>(), It.IsAny<Guid>())).Throws(specificException);

            var results = await _redactionClient.RedactPdfAsync(_request, accessToken, _correlationId);

            results.Should().BeNull();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsSomethingOtherThanNotFound_IsRethrownAsException()
        {
            var accessToken = _fixture.Create<string>();

            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.UnprocessableEntity);
            _mockRequestFactory.Setup(factory => factory.CreatePut($"redactPdf?code={_rumpolePipelineRedactPdfFunctionAppKey}", It.IsAny<string>(), It.IsAny<Guid>())).Throws(specificException);

            var results = async () => await _redactionClient.RedactPdfAsync(_request, accessToken, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }
    }
}
