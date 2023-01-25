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
using Common.Wrappers;
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
    private readonly string _accessToken;
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
        _accessToken = _fixture.Create<string>();
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

        mockHttpRequestFactory.Setup(factory => factory.CreateGet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
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
        var documentStream = await _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _accessToken, _cmsAuthValues, _correlationId);

        documentStream.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDocumentAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
    {
        _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

        await Assert.ThrowsAsync<HttpException>(() => _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _accessToken, _cmsAuthValues, _correlationId));
    }

    [Fact]
    public async Task GetDocumentAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
    {
        const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        _httpResponseMessage.StatusCode = expectedStatusCode;

        try
        {
            await _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _accessToken, _cmsAuthValues, _correlationId);
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
            await _documentExtractionService.GetDocumentAsync(_caseUrn, _caseId, _documentCategory, _documentId, _accessToken, _cmsAuthValues, _correlationId);
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

        await Assert.ThrowsAsync<HttpException>(() => _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId));
    }

    [Fact]
    public async Task ListDocumentsAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
    {
        const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
        _httpResponseMessage.StatusCode = expectedStatusCode;

        try
        {
            await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);
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
            await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);
        }
        catch (HttpException exception)
        {
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsMappedDocuments()
    {
        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);

        result.Length.Should().Be(_content.Count);
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsValidResultsWhen_AllIsWell()
    {
        var searchResults = BuildRandomResults();
        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiCaseDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);

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

        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result[0].DocumentId.Should().Be(searchResults[0].Id.ToString());
            result[0].FileName.Should().Be(searchResults[0].OriginalFileName);
        }
    }

    [Fact]
    public async Task ListDocumentsAsync_ExcludesFromResults_WhenNoFileNamePresent_AndNoDuplicateFoundWithAPopulatedFileName()
    {
        var searchResults = BuildRandomResults();
        searchResults[2].OriginalFileName = null;

        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiCaseDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Length.Should().Be(3);

            foreach (var item in result)
            {
                item.FileName.Should().NotBeNull();
                item.FileName.Length.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public async Task Run_WhenDuplicatesPresent_ReturnsCorrectlyFlattenedResults()
    {
        var searchResults = BuildDuplicateResults();

        _jsonConvertWrapperMock.Setup(x => x.DeserializeObject<IList<DdeiCaseDocumentResponse>>(It.IsAny<string>()))
            .Returns(searchResults);

        var result = await _documentExtractionService.ListDocumentsAsync(_caseUrn, _caseId, _accessToken, _cmsAuthValues, _correlationId);

        using (new AssertionScope())
        {
            result.Length.Should().Be(3);
            var doc1 = result[0];
            var doc2 = result[1];
            var doc3 = result[2];

            doc1.DocumentId.Should().Be("4219309");
            doc1.VersionId.Should().Be(7776580);
            doc1.FileName.Should().Be("PRE-CHARGE CHECKLIST.txt");
            doc1.CmsDocType.Should().NotBeNull();
            doc1.CmsDocType.DocumentCategory.Should().Be("InboxCommunication");
            doc1.CmsDocType.DocumentType.Should().BeNull();
            doc1.CmsDocType.DocumentTypeId.Should().Be("1029");

            doc2.DocumentId.Should().Be("4269468");
            doc2.VersionId.Should().Be(7882834);
            doc2.FileName.Should().Be("MG3221114_164958-26.docx");
            doc2.CmsDocType.Should().NotBeNull();
            doc2.CmsDocType.DocumentCategory.Should().Be("Review");
            doc2.CmsDocType.DocumentType.Should().Be("MG3");
            doc2.CmsDocType.DocumentTypeId.Should().Be("101");

            doc3.DocumentId.Should().Be("4269475");
            doc3.VersionId.Should().Be(7882839);
            doc3.FileName.Should().Be("MG3A221114_165138-121.docx");
            doc3.CmsDocType.Should().NotBeNull();
            doc3.CmsDocType.DocumentCategory.Should().Be("Review");
            doc3.CmsDocType.DocumentType.Should().Be("MG3A");
            doc3.CmsDocType.DocumentTypeId.Should().Be("102");
        }
    }

    private List<DdeiCaseDocumentResponse> BuildDuplicateResults()
    {
        var results = _fixture.CreateMany<DdeiCaseDocumentResponse>(5).ToList();

        results[0].Id = 4269468;
        results[0].VersionId = 7882834;
        results[0].OriginalFileName = null;
        results[0].DocumentType = "MG3";
        results[0].DocumentTypeId = "101";
        results[0].CmsDocCategory = "Review";

        results[1].Id = 4269475;
        results[1].VersionId = 7882839;
        results[1].OriginalFileName = null;
        results[1].DocumentType = "MG3A";
        results[1].DocumentTypeId = "102";
        results[1].CmsDocCategory = "Review";

        results[2].Id = 4219309;
        results[2].VersionId = 7776580;
        results[2].OriginalFileName = "PRE-CHARGE CHECKLIST.txt";
        results[2].DocumentType = null;
        results[2].DocumentTypeId = "1029";
        results[2].CmsDocCategory = "InboxCommunication";

        results[3].Id = 4269468;
        results[3].VersionId = 7882834;
        results[3].OriginalFileName = "MG3221114_164958-26.docx";
        results[3].DocumentType = null;
        results[3].DocumentTypeId = "101";
        results[3].CmsDocCategory = "InboxCommunication";

        results[4].Id = 4269475;
        results[4].VersionId = 7882839;
        results[4].OriginalFileName = "MG3A221114_165138-121.docx";
        results[4].DocumentType = null;
        results[4].DocumentTypeId = "102";
        results[4].CmsDocCategory = "InboxCommunication";

        return results;
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
