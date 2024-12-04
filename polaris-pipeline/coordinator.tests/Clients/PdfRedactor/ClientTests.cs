using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Moq.Protected;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Threading;
using System.Net;
using FluentAssertions;
using Common.Wrappers;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Configuration;
using coordinator.Clients.PdfRedactor;
using System.Text.Json;

namespace coordinator.Tests.Clients.PdfRedactor
{
    public class ClientTests
    {
        private readonly RedactPdfRequestWithDocumentDto _request;
        private readonly Mock<IRequestFactory> _mockRequestFactory;
        private readonly Fixture _fixture;
        private readonly Guid _correlationId;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly string _documentId;
        private readonly IPdfRedactorClient _pdfRedactorClient;

        public ClientTests()
        {
            _fixture = new Fixture();

            _request = _fixture.Create<RedactPdfRequestWithDocumentDto>();
            _mockRequestFactory = new Mock<IRequestFactory>();
            _correlationId = _fixture.Create<Guid>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _documentId = _fixture.Create<string>();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Put
            };

            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}", It.IsAny<Guid>(), null)).Returns(httpRequestMessage);

            var response = _fixture.Create<RedactPdfResponse>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response))
            };

            var stringContent = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfResponse>(stringContent)).Returns(response);
            mockJsonConvertWrapper.Setup(x => x.SerializeObject(It.IsAny<RedactPdfRequestWithDocumentDto>())).Returns(JsonSerializer.Serialize(_request));


            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);
            var redactPdfHttpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _pdfRedactorClient = new PdfRedactorClient(
                _mockRequestFactory.Object,
                redactPdfHttpClient,
                mockJsonConvertWrapper.Object);
        }

        [Fact]
        public async Task RedactPdf_CreatesTheRequestCorrectly()
        {
            await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            _mockRequestFactory.Verify(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}", _correlationId, null));
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrown_IsCaughtAsException()
        {
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}", It.IsAny<Guid>(), null)).Throws<Exception>();

            var results = async () => await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsNotFound_ReturnsNullResponse()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            results.Should().BeNull();
        }

        [Fact]
        public async Task RedactPdf_WhenHttpRequestExceptionThrownAsSomethingOtherThanNotFound_IsRethrownAsException()
        {
            var specificException = new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.UnprocessableEntity);
            _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}", It.IsAny<Guid>(), null)).Throws(specificException);

            var results = async () => await _pdfRedactorClient.RedactPdfAsync(_caseUrn, _caseId, _documentId, _request, _correlationId);

            await results.Should().ThrowAsync<Exception>();
        }
    }
}
