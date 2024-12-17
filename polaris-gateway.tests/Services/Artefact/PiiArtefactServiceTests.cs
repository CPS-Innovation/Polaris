using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.Ocr;
using Common.Domain.Pii;
using Common.Services.BlobStorage;
using Common.Services.PiiService;
using FluentAssertions;
using Moq;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using Xunit;

namespace PolarisGateway.Tests.Services.Artefact;

public class PiiArtefactServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IArtefactServiceResponseFactory> _artefactServiceResponseFactoryMock;
    private readonly Mock<IPiiService> _piiServiceMock;
    private readonly Mock<IOcrArtefactService> _ocrArtefactServiceMock;

    private readonly string _cmsAuthValues;
    private readonly Guid _correlationId;
    private readonly string _urn;
    private readonly int _caseId;
    private readonly long _versionId;
    private readonly string _documentId;

    private readonly PiiArtefactService _piiArtefactService;

    public PiiArtefactServiceTests()
    {
        _fixture = new Fixture();

        _cmsAuthValues = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();
        _urn = _fixture.Create<string>();
        _caseId = _fixture.Create<int>();
        _versionId = _fixture.Create<long>();
        _documentId = _fixture.Create<string>();

        _cacheServiceMock = new Mock<ICacheService>();
        _artefactServiceResponseFactoryMock = new Mock<IArtefactServiceResponseFactory>();
        _piiServiceMock = new Mock<IPiiService>();
        _ocrArtefactServiceMock = new Mock<IOcrArtefactService>();

        _piiArtefactService = new PiiArtefactService(
            _cacheServiceMock.Object,
            _artefactServiceResponseFactoryMock.Object,
            _piiServiceMock.Object,
            _ocrArtefactServiceMock.Object
        );
    }

    [Fact]
    public async Task GetPiiAsync_WhenPiiIsAlreadyInCache_WillReturnCachedPii()
    {
        // Arrange
        var cachedPii = _fixture.CreateMany<PiiLine>();

        _cacheServiceMock
            .Setup(x => x.TryGetJsonObjectAsync<IEnumerable<PiiLine>>(_caseId, _documentId, _versionId, BlobType.Pii))
            .ReturnsAsync((true, cachedPii));

        var expectedResult = new ArtefactResult<IEnumerable<PiiLine>>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(cachedPii, true))
            .Returns(expectedResult);

        // Act
        var result = await _piiArtefactService.GetPiiAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetPiiAsync_WhenOcrIsNotAvailable_WillReturnFailedResult()
    {
        // Arrange
        var ocrResult = new ArtefactResult<AnalyzeResults>
        {
            Status = ResultStatus.Failed
        };

        _ocrArtefactServiceMock
            .Setup(x => x.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, null))
            .ReturnsAsync(ocrResult);

        var expectedResult = new ArtefactResult<IEnumerable<PiiLine>>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.ConvertNonOkResult<AnalyzeResults, IEnumerable<PiiLine>>(ocrResult))
            .Returns(expectedResult);

        // Act
        var result = await _piiArtefactService.GetPiiAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetPiiAsync_WhenOcrIsNotAvailable_WillReturnPollingResult()
    {
        // Arrange
        var ocrResult = new ArtefactResult<AnalyzeResults>
        {
            Status = ResultStatus.PollWithToken,
            ContinuationToken = _fixture.Create<Guid>()
        };

        _ocrArtefactServiceMock
            .Setup(x => x.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, null))
            .ReturnsAsync(ocrResult);

        var expectedResult = new ArtefactResult<IEnumerable<PiiLine>>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.ConvertNonOkResult<AnalyzeResults, IEnumerable<PiiLine>>(ocrResult))
            .Returns(expectedResult);

        // Act
        var result = await _piiArtefactService.GetPiiAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetPiiAsync_WhenPiiIsNotAlreadyInCacheAndConversionSucceeds_WillReturnOkResult()
    {
        // Arrange
        var ocrResult = new ArtefactResult<AnalyzeResults>
        {
            Status = ResultStatus.ArtefactAvailable,
            Artefact = _fixture.Create<AnalyzeResults>()
        };

        _ocrArtefactServiceMock
            .Setup(x => x.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, null))
            .ReturnsAsync(ocrResult);

        var piiResult = _fixture.CreateMany<PiiLine>();

        _piiServiceMock
            .Setup(x => x.GetPiiResultsAsync(ocrResult.Artefact, _correlationId))
            .ReturnsAsync(piiResult);

        var expectedResult = new ArtefactResult<IEnumerable<PiiLine>>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(piiResult, false))
            .Returns(expectedResult);

        // Act
        var result = await _piiArtefactService.GetPiiAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cacheServiceMock.Verify(x => x.UploadJsonObjectAsync(_caseId, _documentId, _versionId, BlobType.Pii, piiResult), Times.Once);
    }
}