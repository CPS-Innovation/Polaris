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
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Configuration;
using Common.Streaming;
using System.IO;
using Common.Clients.PdfGenerator;

namespace coordinator.Tests.Clients.PdfGenerator
{
  public class ClientTests
  {
    private readonly Mock<IPdfGeneratorRequestFactory> _mockRequestFactory;
    private readonly Fixture _fixture;
    private readonly Guid _correlationId;
    private readonly string _caseUrn;
    private readonly int _caseId;
    private readonly string _documentId;
    private readonly long _versionId;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly Mock<IHttpResponseMessageStreamFactory> _mockHttpResponseMessageStreamFactory;
    private readonly HttpRequestMessage _httpRequestMessage;
    private readonly HttpResponseMessage _httpResponseMessage;

    public ClientTests()
    {
      _fixture = new Fixture();

      _fixture.Create<RedactPdfRequestDto>();
      _mockRequestFactory = new Mock<IPdfGeneratorRequestFactory>();
      _correlationId = _fixture.Create<Guid>();
      _caseUrn = _fixture.Create<string>();
      _caseId = _fixture.Create<int>();
      _documentId = _fixture.Create<string>();
      _versionId = _fixture.Create<long>();

      var mockConfiguration = new Mock<IConfiguration>();
      _mockHttpResponseMessageStreamFactory = new Mock<IHttpResponseMessageStreamFactory>();

      _httpRequestMessage = new HttpRequestMessage
      {
        Method = HttpMethod.Put
      };

      _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId, _versionId)}", It.IsAny<Guid>(), null)).Returns(_httpRequestMessage);

      var response = _fixture.Create<RedactPdfResponse>();
      _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(JsonConvert.SerializeObject(response))
      };

      var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
      mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(_httpResponseMessage);
      var redactPdfHttpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

      _pdfGeneratorClient = new PdfGeneratorClient(
          _mockRequestFactory.Object,
          redactPdfHttpClient,
          _mockHttpResponseMessageStreamFactory.Object);
    }

    [Fact]
    public async Task ConvertToPdfAsync_ReturnsAStream()
    {
      // Arrange
      var expectedContent = _fixture.Create<string>();
      _httpResponseMessage.Content = new StringContent(expectedContent);

      _mockRequestFactory
          .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(_caseUrn, _caseId, _documentId, _versionId)}", It.Is<Guid>(g => g == _correlationId), null))
          .Returns(_httpRequestMessage);

      _mockHttpResponseMessageStreamFactory
                      .Setup(factory => factory.Create(It.Is<HttpResponseMessage>(h => h == _httpResponseMessage)))
                      .Returns(HttpResponseMessageStream.Create(_httpResponseMessage));

      // Act
      var response = await _pdfGeneratorClient.ConvertToPdfAsync(_correlationId, _caseUrn, _caseId, _documentId, _versionId, new MemoryStream(), Common.Domain.Document.FileType.MSG);

      // Assert
      var responseText = await new StreamReader(response.PdfStream, System.Text.Encoding.UTF8).ReadToEndAsync();
      responseText.Should().Be(expectedContent);
    }

    [Fact]
    public async Task ConvertToPdfAsync_WhenUnsupportedMediaTypeIsReceived_DoesNotThrowAndReturnsUnsuccessfulResponse()
    {
      // Arrange
      var expectedContent = _fixture.Create<string>();
      _httpResponseMessage.Content = new StringContent(expectedContent);

      _mockRequestFactory
          .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(_caseUrn, _caseId, _documentId, _versionId)}", It.Is<Guid>(g => g == _correlationId), null))
          .Returns(_httpRequestMessage);

      _mockHttpResponseMessageStreamFactory
                      .Setup(factory => factory.Create(It.Is<HttpResponseMessage>(h => h == _httpResponseMessage)))
                      .Returns(HttpResponseMessageStream.Create(_httpResponseMessage));

      _httpResponseMessage.StatusCode = HttpStatusCode.UnsupportedMediaType;

      // Act
      var act = async () => await _pdfGeneratorClient.ConvertToPdfAsync(_correlationId, _caseUrn, _caseId, _documentId, _versionId, new MemoryStream(), Common.Domain.Document.FileType.MSG);

      // Assert
      await act.Should().NotThrowAsync();
    }
    [Fact]
    public async Task ConvertToPdfAsync_WhenHttpRequestExceptionThrown_IsCaughtAsException()
    {
      // Arrange
      _mockRequestFactory
          .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(_caseUrn, _caseId, _documentId, _versionId)}", It.Is<Guid>(g => g == _correlationId), null))
          .Returns(_httpRequestMessage);
      _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

      // Act
      var act = async () => await _pdfGeneratorClient.ConvertToPdfAsync(_correlationId, _caseUrn, _caseId, _documentId, _versionId, new MemoryStream(), Common.Domain.Document.FileType.MSG);


      await act.Should().ThrowAsync<HttpRequestException>();
    }
  }
}
