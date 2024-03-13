using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Moq.Protected;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Threading;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using coordinator.Constants;
using Common.Wrappers;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Configuration;
using coordinator.Clients.PdfRedactor;

namespace coordinator.Tests.Clients.PdfRedactor
{
    public class ClientTests
    {
        private readonly RedactPdfRequestDto _request;
        private readonly Mock<IRequestFactory> _mockRequestFactory;
        private readonly string _polarisPipelineRedactPdfFunctionAppKey;
        private readonly Fixture _fixture;
        private readonly Guid _correlationId;
        private readonly string _caseUrn;
        private readonly string _caseId;
        private readonly string _documentId;
        private readonly IPdfRedactorClient _pdfRedactorClient;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpResponseMessage _httpResponseMessage;

        public ClientTests()
        {
            _fixture = new Fixture();

            _request = _fixture.Create<RedactPdfRequestDto>();
            _mockRequestFactory = new Mock<IRequestFactory>();
            _correlationId = _fixture.Create<Guid>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<string>();
            _documentId = _fixture.Create<string>();

            _polarisPipelineRedactPdfFunctionAppKey = _fixture.Create<string>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

            mockConfiguration.Setup(config => config[ConfigKeys.PipelineRedactorPdfFunctionAppKey]).Returns(_polarisPipelineRedactPdfFunctionAppKey);

            _httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Put
            };

            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Returns(_httpRequestMessage);

            var response = _fixture.Create<RedactPdfResponse>();
            _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };

            var stringContent = _httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfResponse>(stringContent)).Returns(response);
            mockJsonConvertWrapper.Setup(x => x.SerializeObject(It.IsAny<RedactPdfRequestDto>())).Returns(JsonConvert.SerializeObject(_request));


            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_httpResponseMessage);
            var redactPdfHttpClient = new HttpClient(_mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _pdfRedactorClient = new PdfRedactorClient(
                _mockRequestFactory.Object,
                redactPdfHttpClient,
                mockConfiguration.Object,
                mockJsonConvertWrapper.Object);
        }

        [Fact]
        public async Task RedactPdf_CreatesTheRequestCorrectly()
        {
            await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            _mockRequestFactory.Verify(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", _correlationId, null));
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrown_IsCaughtAsException()
        {
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws<Exception>();

            var results = async () => await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsNotFound_ReturnsNullResponse()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            results.Should().BeNull();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsSomethingOtherThanNotFound_IsRethrownAsException()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.UnprocessableEntity);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = async () => await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }
    }
}
