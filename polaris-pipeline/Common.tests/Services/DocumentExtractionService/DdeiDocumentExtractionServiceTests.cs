using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using Common.Constants;
using Common.Domain.Responses;
using Common.Exceptions;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Services.DocumentExtractionService;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Wrappers.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Common.tests.Services.DocumentExtractionService;

public class DdeiDocumentExtractionServiceTests
{
    private readonly Fixture _fixture;
    private readonly string _caseUrn;
    private readonly string _caseId;
    private readonly string _documentCategory;
    private readonly string _documentId;
    private readonly string _cmsAuthValues;
    private readonly Guid _correlationId;
    private readonly HttpResponseMessage _httpResponseMessage;
    private readonly List<DdeiCaseDocumentResponse> _content;
    private readonly Mock<IJsonConvertWrapper> _jsonConvertWrapperMock;

    private readonly IDdeiDocumentExtractionService _documentExtractionService;

    public DdeiDocumentExtractionServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _caseUrn = _fixture.Create<string>();
        _caseId = _fixture.Create<string>();
        _documentCategory = _fixture.Create<string>();
        _documentId = _fixture.Create<string>();
        _cmsAuthValues = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();

        _content = _fixture.CreateMany<DdeiCaseDocumentResponse>(5).ToList();

        var httpRequestMessage = new HttpRequestMessage();
        Stream documentStream = new MemoryStream();

        _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(documentStream)
        };

        var loggerMock = new Mock<ILogger<DdeiDocumentExtractionService>>();

        _jsonConvertWrapperMock = new Mock<IJsonConvertWrapper>();
        _jsonConvertWrapperMock.Setup(wrapper => wrapper.DeserializeObject<List<DdeiCaseDocumentResponse>>(It.IsAny<string>()))
            .Returns(_content);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(_httpResponseMessage);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

        var mockHttpRequestFactory = new Mock<IHttpRequestFactory>();

        mockHttpRequestFactory.Setup(factory => factory.CreateGet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(httpRequestMessage);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.GetDocumentUrl]).Returns($"urns/{0}/cases/{1}/documents/{2}/{3}");
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.ListDocumentsUrl]).Returns($"urns/{0}/cases/{1}/documents");

        _documentExtractionService = new DdeiDocumentExtractionService(httpClient, mockHttpRequestFactory.Object, loggerMock.Object, mockConfiguration.Object,
            _jsonConvertWrapperMock.Object, new DdeiCaseDocumentMapper());
    }

    [Fact]
    public void Ctors_EnsureNotNullAndCorrectExceptionParameterName()
    {
        var assertion = new GuardClauseAssertion(_fixture);
        assertion.Verify(_documentExtractionService.GetType().GetConstructors());
    }

    [Fact]
    public async Task GetDocumentAsync_ReturnsExpectedStream()
    {
        var documentStream = await _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _cmsAuthValues, _correlationId);

        documentStream.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDocumentAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
    {
        _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

        await Assert.ThrowsAsync<HttpException>(() => _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _cmsAuthValues, _correlationId));
    }

    [Fact]
    public async Task GetDocumentAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
    {
        const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        _httpResponseMessage.StatusCode = expectedStatusCode;

        try
        {
            await _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _cmsAuthValues, _correlationId);
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
            await _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _cmsAuthValues, _correlationId);
        }
        catch (HttpException exception)
        {
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
    {
        _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

        await Assert.ThrowsAsync<HttpException>(() => _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId));
    }

    [Fact]
    public async Task ListDocumentsAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
    {
        const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        _httpResponseMessage.StatusCode = expectedStatusCode;

        try
        {
            await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);
        }
        catch (HttpException exception)
        {
            exception.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_HttpExceptionHasHttpRequestExceptionAsInnerExceptionWhenResponseStatusCodeIsNotSuccess()
    {
        _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
        _httpResponseMessage.Content = new StringContent(string.Empty);

        try
        {
            await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);
        }
        catch (HttpException exception)
        {
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsMappedDocuments()
    {
        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);

        result.Length.Should().Be(_content.Count);
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsValidResultsWhen_AllIsWell()
    {
        var searchResults = BuildRandomResults();
        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiCaseDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result[0].DocumentId.Should().Be(searchResults[0].Id.ToString());
            result[0].FileName.Should().Be(searchResults[0].OriginalFileName);
            result[3].DocumentId.Should().Be(searchResults[3].Id.ToString());
            result[3].FileName.Should().Be(searchResults[3].OriginalFileName);
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsPopulatedFilenamesWhen_DuplicateFoundWithAPopulatedFileName()
    {
        var searchResults = BuildRandomResults();
        searchResults[2].Id = 1;
        searchResults[2].OriginalFileName = null;

        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiCaseDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _cmsAuthValues, _correlationId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result[0].DocumentId.Should().Be(searchResults[0].Id.ToString());
            result[0].FileName.Should().Be(searchResults[0].OriginalFileName);
        }
    }

    private List<DdeiCaseDocumentResponse> BuildRandomResults()
    {
        var results = new List<DdeiCaseDocumentResponse>();

        for (var i = 0; i <= 3; i++)
        {
            var baseResponse = _fixture.Create<DdeiCaseDocumentResponse>();
            baseResponse.Id = i + 1;
            results.Add(baseResponse);
        }

        return results;
    }
}
