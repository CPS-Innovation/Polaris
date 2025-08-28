using System;
using System.Collections.Generic;
using AutoFixture;
using Common.Domain.Ocr;
using Common.Services.OcrService;
using Moq;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Factories;
using Xunit;
using Common.Services.BlobStorage;
using PolarisGateway.Services.Artefact.Domain;
using System.Threading.Tasks;
using FluentAssertions;
using System.IO;
using System.Linq;
using System.Threading;
using Common.Dto.Request.Redaction;
using Common.Exceptions;
using Common.Mappers;

namespace PolarisGateway.Tests.Services.Artefact;

public class OcrArtefactServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IArtefactServiceResponseFactory> _artefactServiceResponseFactoryMock;
    private readonly Mock<IOcrService> _ocrServiceMock;
    private readonly Mock<IPdfArtefactService> _pdfArtefactServiceMock;
    private readonly Mock<IRedactionSearchDtoMapper> _redactionSearchDtoMapperMock;

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
        _redactionSearchDtoMapperMock = new Mock<IRedactionSearchDtoMapper>();

        _ocrArtefactService = new OcrArtefactService(
            _cacheServiceMock.Object,
            _artefactServiceResponseFactoryMock.Object,
            _ocrServiceMock.Object,
            _pdfArtefactServiceMock.Object,
            _redactionSearchDtoMapperMock.Object
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
            .Setup(x => x.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false))
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
            .Setup(x => x.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false))
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

    #region GetOcrSearchRedactionsAsync

    [Fact]
    public async Task GetOcrSearchRedactionsAsync_OcrDocumentNotFound_ShouldThrowOcrDocumentNotFoundException()
    {
        //arrange
        var urn = "Urn";
        var caseId = 1;
        var documentId = "2";
        var versionId = 3;
        var searchTerm = "search";
        var cancellationToken = CancellationToken.None;
        var results = new AnalyzeResults();
        _cacheServiceMock.Setup(s => s.TryGetJsonObjectAsync<AnalyzeResults>(caseId, documentId, versionId, BlobType.Ocr)).ReturnsAsync((false, results));

        //act & Assert
        await Assert.ThrowsAsync<OcrDocumentNotFoundException>(() => _ocrArtefactService.GetOcrSearchRedactionsAsync(_cmsAuthValues, _correlationId, urn, caseId, documentId, versionId, searchTerm, cancellationToken));
    }

    [Fact]
    public async Task GetOcrSearchRedactionsAsync_SearchFoundOnSamePage_ShouldReturnFoundSearchTerms()
    {
        //arrange
        var urn = "Urn";
        var caseId = 1;
        var documentId = "2";
        var versionId = 3;
        var searchTerm = "Hello World";
        var cancellationToken = CancellationToken.None;
        var results = new AnalyzeResults();
        var redactionSearchDtos = new List<RedactionSearchDto>()
        {
            new RedactionSearchDto()
            {
                Height = 1,
                PageIndex = 1,
                Width = 1,
                Word = "Hello",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 1,
                    Y1 = 2,
                    X2 = 3,
                    Y2 = 4
                }
            },
            new RedactionSearchDto()
            {
                Height = 1,
                PageIndex = 1,
                Width = 1,
                Word = "World",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 5,
                    Y1 = 6,
                    X2 = 7,
                    Y2 = 8
                }
            },
        };
        _cacheServiceMock.Setup(s => s.TryGetJsonObjectAsync<AnalyzeResults>(caseId, documentId, versionId, BlobType.Ocr)).ReturnsAsync((true, results));
        _redactionSearchDtoMapperMock.Setup(s => s.Map(results.ReadResults)).Returns(redactionSearchDtos);
        
        //act
        var result = (await _ocrArtefactService.GetOcrSearchRedactionsAsync(_cmsAuthValues, _correlationId, urn, caseId, documentId, versionId, searchTerm, cancellationToken)).ToList();

        //assert
        Assert.Single(result);
        Assert.Equal(result[0].Height, redactionSearchDtos[0].Height);
        Assert.Equal(result[0].PageIndex, redactionSearchDtos[0].PageIndex);
        Assert.Equal(result[0].Width, redactionSearchDtos[0].Width);
        Assert.Equal(result[0].RedactionCoordinates[0].X1, redactionSearchDtos[0].RedactionCoordinates.X1);
        Assert.Equal(result[0].RedactionCoordinates[0].X2, redactionSearchDtos[0].RedactionCoordinates.X2);
        Assert.Equal(result[0].RedactionCoordinates[0].Y1, redactionSearchDtos[0].RedactionCoordinates.Y1);
        Assert.Equal(result[0].RedactionCoordinates[0].Y2, redactionSearchDtos[0].RedactionCoordinates.Y2);
        Assert.Equal(result[0].RedactionCoordinates[1].X1, redactionSearchDtos[1].RedactionCoordinates.X1);
        Assert.Equal(result[0].RedactionCoordinates[1].X2, redactionSearchDtos[1].RedactionCoordinates.X2);
        Assert.Equal(result[0].RedactionCoordinates[1].Y1, redactionSearchDtos[1].RedactionCoordinates.Y1);
        Assert.Equal(result[0].RedactionCoordinates[1].Y2, redactionSearchDtos[1].RedactionCoordinates.Y2);
    }
    
    [Fact]
    public async Task GetOcrSearchRedactionsAsync_SearchFoundOverDifferentPages_ShouldReturnFoundSearchTerms()
    {
        //arrange
        var urn = "Urn";
        var caseId = 1;
        var documentId = "2";
        var versionId = 3;
        var searchTerm = "Hello World";
        var cancellationToken = CancellationToken.None;
        var results = new AnalyzeResults();
        var redactionSearchDtos = new List<RedactionSearchDto>()
        {
            new RedactionSearchDto()
            {
                Height = 1,
                PageIndex = 1,
                Width = 2,
                Word = "Hello",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 1,
                    Y1 = 2,
                    X2 = 3,
                    Y2 = 4
                }
            },
            new RedactionSearchDto()
            {
                Height = 3,
                PageIndex = 2,
                Width = 4,
                Word = "World",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 5,
                    Y1 = 6,
                    X2 = 7,
                    Y2 = 8
                }
            },
        };
        _cacheServiceMock.Setup(s => s.TryGetJsonObjectAsync<AnalyzeResults>(caseId, documentId, versionId, BlobType.Ocr)).ReturnsAsync((true, results));
        _redactionSearchDtoMapperMock.Setup(s => s.Map(results.ReadResults)).Returns(redactionSearchDtos);
        
        //act
        var result = (await _ocrArtefactService.GetOcrSearchRedactionsAsync(_cmsAuthValues, _correlationId, urn, caseId, documentId, versionId, searchTerm, cancellationToken)).ToList();

        //assert
        Assert.Equal(2, result.Count);
        Assert.Equal(result[0].Height, redactionSearchDtos[0].Height);
        Assert.Equal(result[0].PageIndex, redactionSearchDtos[0].PageIndex);
        Assert.Equal(result[0].Width, redactionSearchDtos[0].Width);
        Assert.Equal(result[0].RedactionCoordinates[0].X1, redactionSearchDtos[0].RedactionCoordinates.X1);
        Assert.Equal(result[0].RedactionCoordinates[0].X2, redactionSearchDtos[0].RedactionCoordinates.X2);
        Assert.Equal(result[0].RedactionCoordinates[0].Y1, redactionSearchDtos[0].RedactionCoordinates.Y1);
        Assert.Equal(result[0].RedactionCoordinates[0].Y2, redactionSearchDtos[0].RedactionCoordinates.Y2);
        Assert.Equal(result[1].Height, redactionSearchDtos[1].Height);
        Assert.Equal(result[1].PageIndex, redactionSearchDtos[1].PageIndex);
        Assert.Equal(result[1].Width, redactionSearchDtos[1].Width);
        Assert.Equal(result[1].RedactionCoordinates[0].X1, redactionSearchDtos[1].RedactionCoordinates.X1);
        Assert.Equal(result[1].RedactionCoordinates[0].X2, redactionSearchDtos[1].RedactionCoordinates.X2);
        Assert.Equal(result[1].RedactionCoordinates[0].Y1, redactionSearchDtos[1].RedactionCoordinates.Y1);
        Assert.Equal(result[1].RedactionCoordinates[0].Y2, redactionSearchDtos[1].RedactionCoordinates.Y2);
    }


    #endregion
}