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
using coordinator.Clients.Contracts;
using coordinator.Constants;
using Common.Wrappers.Contracts;
using Common.Factories.Contracts;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Configuration;
using Common.Streaming;
using System.IO;

namespace coordinator.Clients.Tests.Clients
{
    public class PdfGeneratorClientTests
    {
        private readonly RedactPdfRequestDto _request;
        private readonly Mock<IPipelineClientRequestFactory> _mockRequestFactory;
        private readonly string _polarisPipelineRedactPdfFunctionAppKey;
        private readonly Fixture _fixture;
        private readonly Guid _correlationId;
        private readonly string _caseUrn;
        private readonly string _caseId;
        private readonly string _documentId;
        private readonly string _versionId;
        private readonly IPdfGeneratorClient _pdfGeneratorClient;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<IHttpResponseMessageStreamFactory> _mockHttpResponseMessageStreamFactory;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpResponseMessage _httpResponseMessage;

        public PdfGeneratorClientTests()
        {
            _fixture = new Fixture();

            _request = _fixture.Create<RedactPdfRequestDto>();
            _mockRequestFactory = new Mock<IPipelineClientRequestFactory>();
            _correlationId = _fixture.Create<Guid>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<string>();
            _documentId = _fixture.Create<string>();
            _versionId = _fixture.Create<string>();

            _polarisPipelineRedactPdfFunctionAppKey = _fixture.Create<string>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockHttpResponseMessageStreamFactory = new Mock<IHttpResponseMessageStreamFactory>();

            mockConfiguration.Setup(config => config[ConfigKeys.PipelineRedactPdfFunctionAppKey]).Returns(_polarisPipelineRedactPdfFunctionAppKey);

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
            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfResponse>(stringContent, It.IsAny<Guid>())).Returns(response);
            mockJsonConvertWrapper.Setup(x => x.SerializeObject(It.IsAny<RedactPdfRequestDto>(), It.IsAny<Guid>())).Returns(JsonConvert.SerializeObject(_request));

            var mockRedactionClientLogger = new Mock<ILogger<PdfGeneratorClient>>();

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_httpResponseMessage);
            var redactPdfHttpClient = new HttpClient(_mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _pdfGeneratorClient = new PdfGeneratorClient(
                _mockRequestFactory.Object,
                redactPdfHttpClient,
                mockConfiguration.Object,
                _mockHttpResponseMessageStreamFactory.Object,
                mockJsonConvertWrapper.Object);
        }

        [Fact]
        public async Task RedactPdf_CreatesTheRequestCorrectly()
        {
            await _pdfGeneratorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            _mockRequestFactory.Verify(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", _correlationId, null));
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrown_IsCaughtAsException()
        {
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws<Exception>();

            var results = async () => await _pdfGeneratorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsNotFound_ReturnsNullResponse()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = await _pdfGeneratorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            results.Should().BeNull();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsSomethingOtherThanNotFound_IsRethrownAsException()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.UnprocessableEntity);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = async () => await _pdfGeneratorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ConvertToPdfAsync_ReturnsAStream()
        {
            // Arrange
            var expectedContent = _fixture.Create<string>();
            _httpResponseMessage.Content = new StringContent(expectedContent);

            _mockRequestFactory
                .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(_caseUrn, _caseId, _documentId, _versionId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.Is<Guid>(g => g == _correlationId), null))
                .Returns(_httpRequestMessage);

            _mockHttpResponseMessageStreamFactory
                            .Setup(factory => factory.Create(It.Is<HttpResponseMessage>(h => h == _httpResponseMessage)))
                            .Returns(HttpResponseMessageStream.Create(_httpResponseMessage));

            // Act
            var response = await _pdfGeneratorClient.ConvertToPdfAsync(_correlationId, string.Empty, _caseUrn, _caseId, _documentId, _versionId, new MemoryStream(), Common.Domain.Document.FileType.MSG);

            // Assert
            var responseText = new StreamReader(response, System.Text.Encoding.UTF8).ReadToEnd();
            responseText.Should().Be(expectedContent);
        }

        [Fact]
        public async Task ConvertToPdfAsync_WhenUnsupportedMediaTypeIsReceived_DoesNotThrowAndReturnsUnsuccessfulResponse()
        {
            // Arrange
            var expectedContent = _fixture.Create<string>();
            _httpResponseMessage.Content = new StringContent(expectedContent);

            _mockRequestFactory
                .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(_caseUrn, _caseId, _documentId, _versionId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.Is<Guid>(g => g == _correlationId), null))
                .Returns(_httpRequestMessage);

            _mockHttpResponseMessageStreamFactory
                            .Setup(factory => factory.Create(It.Is<HttpResponseMessage>(h => h == _httpResponseMessage)))
                            .Returns(HttpResponseMessageStream.Create(_httpResponseMessage));

            _httpResponseMessage.StatusCode = HttpStatusCode.UnsupportedMediaType;

            // Act
            var act = async () => await _pdfGeneratorClient.ConvertToPdfAsync(_correlationId, string.Empty, _caseUrn, _caseId, _documentId, _versionId, new MemoryStream(), Common.Domain.Document.FileType.MSG);

            // Assert
            await act.Should().ThrowAsync<UnsupportedMediaTypeException>();
        }
        [Fact]
        public async Task ConvertToPdfAsync_WhenHttpRequestExceptionThrown_IsCaughtAsException()
        {
            // Arrange
            _mockRequestFactory
                .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(_caseUrn, _caseId, _documentId, _versionId)}?code={_polarisPipelineRedactPdfFunctionAppKey}", It.Is<Guid>(g => g == _correlationId), null))
                .Returns(_httpRequestMessage);
            _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

            // Act
            var act = async () => await _pdfGeneratorClient.ConvertToPdfAsync(_correlationId, string.Empty, _caseUrn, _caseId, _documentId, _versionId, new System.IO.MemoryStream(), Common.Domain.Document.FileType.MSG);


            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
