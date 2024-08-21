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
using coordinator.Clients.PdfGenerator;
using Common.Wrappers;

namespace coordinator.Tests.Clients.PdfGenerator
{
  public class ClientTests
  {
    private readonly Mock<IRequestFactory> _mockRequestFactory;
    private readonly Fixture _fixture;
    private readonly Guid _correlationId;
    private readonly string _caseUrn;
    private readonly string _caseId;
    private readonly string _documentId;
    private readonly string _versionId;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly Mock<IHttpResponseMessageStreamFactory> _mockHttpResponseMessageStreamFactory;
    private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
    private readonly HttpRequestMessage _httpRequestMessage;
    private readonly HttpResponseMessage _httpResponseMessage;

    public ClientTests()
    {
      _fixture = new Fixture();

      _fixture.Create<RedactPdfRequestDto>();
      _mockRequestFactory = new Mock<IRequestFactory>();
      _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
      _correlationId = _fixture.Create<Guid>();
      _caseUrn = _fixture.Create<string>();
      _caseId = _fixture.Create<string>();
      _documentId = _fixture.Create<string>();
      _versionId = _fixture.Create<string>();

      var mockConfiguration = new Mock<IConfiguration>();
      _mockHttpResponseMessageStreamFactory = new Mock<IHttpResponseMessageStreamFactory>();

      _httpRequestMessage = new HttpRequestMessage
      {
        Method = HttpMethod.Put
      };

      _mockRequestFactory.Setup(factory => factory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(_caseUrn, _caseId, _documentId)}", It.IsAny<Guid>(), null)).Returns(_httpRequestMessage);

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
          _mockHttpResponseMessageStreamFactory.Object,
          _mockJsonConvertWrapper.Object);
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

    [Fact]
    public async Task GenerateThumbnail_ReturnsStream()
    {
      // Arrange
      var expectedStreamContent = _fixture.Create<string>();
      _httpResponseMessage.Content = new StringContent(expectedStreamContent);
      _httpResponseMessage.StatusCode = HttpStatusCode.OK;

      _mockRequestFactory
          .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetGenerateThumbnailPath(_caseUrn, _caseId, _documentId)}", It.Is<Guid>(g => g == _correlationId), null))
          .Returns(_httpRequestMessage);

      var thumbnailRequest = _fixture.Create<GenerateThumbnailWithDocumentDto>();
      _mockJsonConvertWrapper
          .Setup(wrapper => wrapper.SerializeObject(thumbnailRequest))
          .Returns(JsonConvert.SerializeObject(thumbnailRequest));

      // Act
      var resultStream = await _pdfGeneratorClient.GenerateThumbnail(_caseUrn, _caseId, _documentId, thumbnailRequest, _correlationId);

      // Assert
      var resultText = await new StreamReader(resultStream).ReadToEndAsync();
      resultText.Should().Be(expectedStreamContent);
    }

    [Fact]
    public async Task GenerateThumbnail_ThrowsHttpRequestException_WhenResponseIsUnsuccessful()
    {
      // Arrange
      var expectedStreamContent = _fixture.Create<string>();
      _httpResponseMessage.Content = new StringContent(expectedStreamContent);
      _httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;

      _mockRequestFactory
          .Setup(factory => factory.Create(HttpMethod.Post, $"{RestApi.GetGenerateThumbnailPath(_caseUrn, _caseId, _documentId)}", It.Is<Guid>(g => g == _correlationId), null))
          .Returns(_httpRequestMessage);

      var thumbnailRequest = _fixture.Create<GenerateThumbnailWithDocumentDto>();
      _mockJsonConvertWrapper
          .Setup(wrapper => wrapper.SerializeObject(thumbnailRequest))
          .Returns(JsonConvert.SerializeObject(thumbnailRequest));

      // Act
      Func<Task> act = async () => await _pdfGeneratorClient.GenerateThumbnail(_caseUrn, _caseId, _documentId, thumbnailRequest, _correlationId);

      // Assert
      await act.Should().ThrowAsync<HttpRequestException>();
    }
  }
}
