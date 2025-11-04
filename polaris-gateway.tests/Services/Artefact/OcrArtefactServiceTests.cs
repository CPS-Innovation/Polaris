using AutoFixture;
using Common.Domain.Ocr;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using FluentAssertions;
using Moq;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Services.Artefact;

public class OcrArtefactServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IArtefactServiceResponseFactory> _artefactServiceResponseFactoryMock;
    private readonly Mock<IOcrService> _ocrServiceMock;
    private readonly Mock<IPdfArtefactService> _pdfArtefactServiceMock;

    private readonly string _cmsAuthValues;
    private readonly Guid _correlationId;
    private readonly string _urn;
    private readonly int _caseId;
    private readonly long _versionId;
    private readonly string _documentId;

    private readonly OcrArtefactService _ocrArtefactService;

    public OcrArtefactServiceTests()
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
        _ocrServiceMock = new Mock<IOcrService>();
        _pdfArtefactServiceMock = new Mock<IPdfArtefactService>();

        _ocrArtefactService = new OcrArtefactService(
            _cacheServiceMock.Object,
            _artefactServiceResponseFactoryMock.Object,
            _ocrServiceMock.Object,
            _pdfArtefactServiceMock.Object
        );
    }

    #region GetOcrAsync
    [Fact]
    public async Task GetOcrAsync_WhenOcrIsAlreadyInCache_WillReturnCachedOcr()
    {
        // Arrange
        var cachedOcrResult = _fixture.Create<AnalyzeResults>();

        _cacheServiceMock
            .Setup(x => x.TryGetJsonObjectAsync<AnalyzeResults>(_caseId, _documentId, _versionId, BlobType.Ocr))
            .ReturnsAsync((true, cachedOcrResult));

        var expectedResult = new ArtefactResult<AnalyzeResults>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(cachedOcrResult, true))
            .Returns(expectedResult);

        // Act
        var result = await _ocrArtefactService.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetOcrAsync_WhenOperationIdIsProvided_WillReturnResultsIfOcrIsComplete()
    {
        // Arrange

        _cacheServiceMock
            .Setup(x => x.TryGetJsonObjectAsync<AnalyzeResults>(_caseId, _documentId, _versionId, BlobType.Ocr))
            .ReturnsAsync((false, null));

        var operationId = _fixture.Create<Guid>();

        var analyzeResults = _fixture.Create<AnalyzeResults>();
        _ocrServiceMock
            .Setup(x => x.GetOperationResultsAsync(operationId, _correlationId))
            .ReturnsAsync(new OcrOperationResult
            {
                IsSuccess = true,
                AnalyzeResults = analyzeResults
            });

        _cacheServiceMock
            .Setup(x => x.UploadJsonObjectAsync(_caseId, _documentId, _versionId, BlobType.Ocr, analyzeResults))
            .Returns(Task.CompletedTask);

        var expectedResult = new ArtefactResult<AnalyzeResults>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(analyzeResults, false))
            .Returns(expectedResult);

        // Act
        var result = await _ocrArtefactService.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, operationId);

        // Assert
        result.Should().Be(expectedResult);
        _cacheServiceMock.Verify(x => x.UploadJsonObjectAsync(_caseId, _documentId, _versionId, BlobType.Ocr, analyzeResults), Times.Once);
    }

    [Fact]
    public async Task GetOcrAsync_WhenOperationIdIsProvided_WillReturnOperationIdIfOcrIsInProgress()
    {
        // Arrange
        _cacheServiceMock
            .Setup(x => x.TryGetJsonObjectAsync<AnalyzeResults>(_caseId, _documentId, _versionId, BlobType.Ocr))
            .ReturnsAsync((false, null));

        var operationId = _fixture.Create<Guid>();

        _ocrServiceMock
            .Setup(x => x.GetOperationResultsAsync(operationId, _correlationId))
            .ReturnsAsync(new OcrOperationResult
            {
                IsSuccess = false
            });

        var expectedResult = new ArtefactResult<AnalyzeResults>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateInterimResult<AnalyzeResults>(operationId))
            .Returns(expectedResult);

        // Act
        var result = await _ocrArtefactService.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, operationId);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetOcrAsync_WhenOperationIdIsNotProvidedAndPdfIsNotAvailable_WillReturnFailedResult()
    {
        // Arrange
        _cacheServiceMock
            .Setup(x => x.TryGetJsonObjectAsync<AnalyzeResults>(_caseId, _documentId, _versionId, BlobType.Ocr))
            .ReturnsAsync((false, null));

        var pdfResult = new ArtefactResult<Stream>()
        {
            Status = ResultStatus.Failed
        };

        _pdfArtefactServiceMock
            .Setup(x => x.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, false))
            .ReturnsAsync(pdfResult);

        var expectedResult = new ArtefactResult<AnalyzeResults>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.ConvertNonOkResult<Stream, AnalyzeResults>(pdfResult))
            .Returns(expectedResult);

        // Act
        var result = await _ocrArtefactService.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetOcrAsync_WhenOperationIdIsNotProvidedAndPdfIsAvailable_WillInitiateOcrOperation()
    {
        // Arrange
        _cacheServiceMock
            .Setup(x => x.TryGetJsonObjectAsync<AnalyzeResults>(_caseId, _documentId, _versionId, BlobType.Ocr))
            .ReturnsAsync((false, null));

        var pdfResult = new ArtefactResult<Stream>()
        {
            Status = ResultStatus.ArtefactAvailable,
            Artefact = new MemoryStream(_fixture.Create<byte[]>())
        };

        _pdfArtefactServiceMock
            .Setup(x => x.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false, false))
            .ReturnsAsync(pdfResult);

        var newOperationId = _fixture.Create<Guid>();

        _ocrServiceMock
            .Setup(x => x.InitiateOperationAsync(pdfResult.Artefact, _correlationId))
            .ReturnsAsync(newOperationId);

        var expectedResult = new ArtefactResult<AnalyzeResults>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateInterimResult<AnalyzeResults>(newOperationId))
            .Returns(expectedResult);

        // Act
        var result = await _ocrArtefactService.GetOcrAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);
    }
    #endregion
}