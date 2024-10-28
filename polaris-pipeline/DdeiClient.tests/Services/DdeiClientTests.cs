using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using System.Linq;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Mappers;
using Ddei.Exceptions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using Ddei.Domain.Response.Document;

namespace Ddei.tests.Services;

public class DdeiClientTests
{
    private readonly Fixture _fixture;
    private readonly DdeiCaseIdentifiersArgDto _ddeiCaseIdentifiersArgDto;
    private readonly HttpResponseMessage _httpResponseMessage;
    private readonly List<DdeiDocumentResponse> _content;
    private readonly Mock<IJsonConvertWrapper> _jsonConvertWrapperMock;
    private readonly DdeiClient _ddeiClient;

    public DdeiClientTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        var caseUrn = _fixture.Create<string>();
        var caseId = _fixture.Create<int>().ToString();
        var cmsAuthValues = _fixture.Create<string>();
        var correlationId = _fixture.Create<Guid>();

        _ddeiCaseIdentifiersArgDto = _fixture.Create<DdeiCaseIdentifiersArgDto>();

        _content = _fixture.CreateMany<DdeiDocumentResponse>(5).ToList();

        var httpRequestMessage = new HttpRequestMessage();
        Stream documentStream = new MemoryStream();

        _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(documentStream)
        };

        var loggerMock = new Mock<ILogger<DdeiClient>>();

        _jsonConvertWrapperMock = new Mock<IJsonConvertWrapper>();
        _jsonConvertWrapperMock.Setup(wrapper => wrapper.DeserializeObject<List<DdeiDocumentResponse>>(It.IsAny<string>()))
            .Returns(_content);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(_httpResponseMessage);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

        var mockHttpRequestFactory = new Mock<IDdeiClientRequestFactory>();

        mockHttpRequestFactory
            .Setup(factory => factory.CreateListCaseDocumentsRequest(It.IsAny<DdeiCaseIdentifiersArgDto>()))
            .Returns(httpRequestMessage);

        var mockConfiguration = new Mock<IConfiguration>();
        var mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
        var mockCaseDetailsMapper = new Mock<ICaseDetailsMapper>();
        var mockCaseDocumentNoteMapper = new Mock<ICaseDocumentNoteMapper>();
        var mockCaseDocumentNoteResultMapper = new Mock<ICaseDocumentNoteResultMapper>();
        var mockCaseExhibitProducerMapper = new Mock<ICaseExhibitProducerMapper>();
        var mockCaseWitnessMapper = new Mock<ICaseWitnessMapper>();
        var mockCaseIdentifiersMapper = new Mock<ICaseIdentifiersMapper>();
        var mockCmsMaterialTypeMapper = new Mock<ICmsMaterialTypeMapper>();
        var mockCaseWitnessStatementMapper = new Mock<ICaseWitnessStatementMapper>();

        _ddeiClient = new DdeiClient
            (
                httpClient,
                mockHttpRequestFactory.Object,
                mockDdeiArgFactory.Object,
                mockCaseDetailsMapper.Object,
                new CaseDocumentMapper(),
                mockCaseDocumentNoteMapper.Object,
                mockCaseDocumentNoteResultMapper.Object,
                mockCaseExhibitProducerMapper.Object,
                mockCaseWitnessMapper.Object,
                mockCaseIdentifiersMapper.Object,
                mockCmsMaterialTypeMapper.Object,
                mockCaseWitnessStatementMapper.Object,
                _jsonConvertWrapperMock.Object,
                loggerMock.Object
            );
    }

    [Fact]
    public void Ctors_EnsureNotNullAndCorrectExceptionParameterName()
    {
        var assertion = new GuardClauseAssertion(_fixture);
        assertion.Verify(_ddeiClient.GetType().GetConstructors());
    }

    [Fact]
    public async Task ListDocumentsAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
    {
        _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

        await Assert.ThrowsAsync<DdeiClientException>(() => _ddeiClient.ListDocumentsAsync(_ddeiCaseIdentifiersArgDto));
    }

    [Fact]
    public async Task ListDocumentsAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
    {
        const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        _httpResponseMessage.StatusCode = expectedStatusCode;

        try
        {
            await _ddeiClient.ListDocumentsAsync(_ddeiCaseIdentifiersArgDto);
        }
        catch (DdeiClientException exception)
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
            await _ddeiClient.ListDocumentsAsync(_ddeiCaseIdentifiersArgDto);
        }
        catch (DdeiClientException exception)
        {
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsMappedDocuments()
    {
        var result = await _ddeiClient.ListDocumentsAsync(_ddeiCaseIdentifiersArgDto);

        result.Count().Should().Be(_content.Count);
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsValidResultsWhen_AllIsWell()
    {
        var searchResults = BuildRandomResults();
        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _ddeiClient.ListDocumentsAsync(_ddeiCaseIdentifiersArgDto);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.First().DocumentId.Should().Be(searchResults[0].Id);
            result.First().FileName.Should().Be(searchResults[0].OriginalFileName);
            result.First().PresentationTitle.Should().Be(searchResults[0].PresentationTitle);
            result.ElementAt(3).DocumentId.Should().Be(searchResults[3].Id);
            result.ElementAt(3).FileName.Should().Be(searchResults[3].OriginalFileName);
            result.ElementAt(3).PresentationTitle.Should().Be(searchResults[3].PresentationTitle);
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsPopulatedFilenamesWhen_DuplicateFoundWithAPopulatedFileName()
    {
        var searchResults = BuildRandomResults();
        searchResults[2].Id = 1;
        searchResults[2].OriginalFileName = null;
        searchResults[2].PresentationTitle = null;

        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _ddeiClient.ListDocumentsAsync(_ddeiCaseIdentifiersArgDto);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.First().DocumentId.Should().Be(searchResults[0].Id);
            result.First().FileName.Should().Be(searchResults[0].OriginalFileName);
            result.First().PresentationTitle.Should().Be(searchResults[0].PresentationTitle);
        }
    }

    private List<DdeiDocumentResponse> BuildRandomResults()
    {
        var results = new List<DdeiDocumentResponse>();

        for (var i = 0; i <= 3; i++)
        {
            var baseResponse = _fixture.Create<DdeiDocumentResponse>();
            baseResponse.Id = i + 1;
            results.Add(baseResponse);
        }

        return results;
    }
}