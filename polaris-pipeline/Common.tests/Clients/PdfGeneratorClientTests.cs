using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Common.Clients.Contracts;
using Common.Wrappers.Contracts;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Dto.Request;
using Common.Dto.Response;

namespace Common.Clients.Tests.Clients
{
    public class PdfGeneratorClientTests
    {
        private readonly RedactPdfRequestDto _request;
        private readonly Mock<IPipelineClientRequestFactory> _mockRequestFactory;
        private readonly string _polarisPipelineRedactPdfFunctionAppKey;
        private readonly Fixture _fixture;
        private readonly Guid _correlationId;

        private readonly IPdfGeneratorClient _redactionClient;

        public PdfGeneratorClientTests()
        {
            _fixture = new Fixture();

            _request = _fixture.Create<RedactPdfRequestDto>();
            _mockRequestFactory = new Mock<IPipelineClientRequestFactory>();
            _correlationId = _fixture.Create<Guid>();

            _polarisPipelineRedactPdfFunctionAppKey = _fixture.Create<string>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

            mockConfiguration.Setup(config => config[PipelineSettings.PipelineRedactPdfFunctionAppKey]).Returns(_polarisPipelineRedactPdfFunctionAppKey);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Put
            };

            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"redact-pdf?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Returns(httpRequestMessage);

            var redactPdfResponse = _fixture.Create<RedactPdfResponse>();
            var redactPdfResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(redactPdfResponse))
            };

            var stringContent = redactPdfResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfResponse>(stringContent, It.IsAny<Guid>())).Returns(redactPdfResponse);
            mockJsonConvertWrapper.Setup(x => x.SerializeObject(It.IsAny<RedactPdfRequestDto>(), It.IsAny<Guid>())).Returns(JsonConvert.SerializeObject(_request));

            var mockRedactionClientLogger = new Mock<ILogger<PdfGeneratorClient>>();

            var mockRedactPdfMessageHandler = new Mock<HttpMessageHandler>();
            mockRedactPdfMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(redactPdfResponseMessage);
            var redactPdfHttpClient = new HttpClient(mockRedactPdfMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _redactionClient = new PdfGeneratorClient(
                _mockRequestFactory.Object,
                redactPdfHttpClient,
                mockConfiguration.Object,
                mockJsonConvertWrapper.Object,
                mockRedactionClientLogger.Object);
        }

        [Fact]
        public async Task RedactPdf_CreatesTheRequestCorrectly()
        {
            await _redactionClient.RedactPdfAsync(_request, _correlationId);

            _mockRequestFactory.Verify(factory => factory.Create(HttpMethod.Put, $"redact-pdf?code={_polarisPipelineRedactPdfFunctionAppKey}", _correlationId, null));
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrown_IsCaughtAsException()
        {
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"redact-pdf?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws<Exception>();

            var results = async () => await _redactionClient.RedactPdfAsync(_request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsNotFound_ReturnsNullResponse()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"redact-pdf?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = await _redactionClient.RedactPdfAsync(_request, _correlationId);

            results.Should().BeNull();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsSomethingOtherThanNotFound_IsRethrownAsException()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.UnprocessableEntity);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"redact-pdf?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = async () => await _redactionClient.RedactPdfAsync(_request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }
    }
}
