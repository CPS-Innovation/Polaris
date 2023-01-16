using System.Net;
using AutoFixture;
using Common.Constants;
using Common.Exceptions;
using Common.Factories.Contracts;
using Common.Services.DocumentExtractionService;
using Common.Services.DocumentExtractionService.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Common.tests.Services.DocumentExtractionService
{
	public class CgiDocumentExtractionServiceTests
	{
        private readonly string _documentId;
        private readonly string _fileName;
        private readonly string _accessToken;
        private readonly string _upstreamToken;
        private readonly Guid _correlationId;
        private readonly HttpResponseMessage _httpResponseMessage;

        private readonly ICgiDocumentExtractionService _documentExtractionService;

        public CgiDocumentExtractionServiceTests()
        {
            var fixture = new Fixture();
            _documentId = fixture.Create<string>();
            _fileName = fixture.Create<string>();
            _accessToken = fixture.Create<string>();
            _upstreamToken = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();
            var httpRequestMessage = new HttpRequestMessage();
            Stream documentStream = new MemoryStream();
            _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(documentStream)
            };

            var loggerMock = new Mock<ILogger<CgiDocumentExtractionService>>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_httpResponseMessage);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            var mockHttpRequestFactory = new Mock<IHttpRequestFactory>();

            mockHttpRequestFactory.Setup(factory => factory.CreateGet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<Guid>()))
                .Returns(httpRequestMessage);
            
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.GetDocumentUrl]).Returns($"doc-fetch/{0}/{1}");
            
            _documentExtractionService = new CgiDocumentExtractionService(httpClient, mockHttpRequestFactory.Object, loggerMock.Object, mockConfiguration.Object);
        }

        [Fact]
        public async Task GetDocumentAsync_ReturnsExpectedStream()
        {
            var documentStream = await _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken, _correlationId);

            documentStream.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
        {
            _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

            await Assert.ThrowsAsync<HttpException>(() => _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken, _correlationId));
        }

        [Fact]
        public async Task GetDocumentAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
            _httpResponseMessage.StatusCode = expectedStatusCode;

            try
            {
                await _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken, _correlationId);
            }
            catch (HttpException exception)
            {
                exception.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Fact]
        public async Task GetDocumentAsync_HttpExceptionHasHttpRequestExceptionAsInnerExceptionWhenResponseStatusCodeIsNotSuccess()
        {
            _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            _httpResponseMessage.Content = new StringContent(string.Empty);

            try
            {
                await _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken, _correlationId);
            }
            catch (HttpException exception)
            {
                exception.InnerException.Should().BeOfType<HttpRequestException>();
            }
        }
    }
}

