using Aspose.Pdf.Annotations;
using AutoFixture;
using Common.Constants;
using Common.Domain.Ocr;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Services.Artefact;

public class PdfArtefactServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IArtefactServiceResponseFactory> _artefactServiceResponseFactoryMock;
    private readonly Mock<IPdfRetrievalService> _pdfRetrievalServiceMock;
    private readonly string _cmsAuthValues;
    private readonly Guid _correlationId;
    private readonly string _urn;
    private readonly int _caseId;
    private readonly long _versionId;
    private readonly string _documentId;
    private readonly PdfArtefactService _pdfArtefactService;
    private readonly Mock<IOcrService> _ocrServiceMock;
    public PdfArtefactServiceTests()
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
        _pdfRetrievalServiceMock = new Mock<IPdfRetrievalService>();
        _ocrServiceMock = new Mock<IOcrService>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "FileSizeLimitMb", "10" },
            })
            .Build();

        _pdfArtefactService = new PdfArtefactService(
            new Mock<ILogger<PdfArtefactService>>().Object,
            _cacheServiceMock.Object,
            _artefactServiceResponseFactoryMock.Object,
            _pdfRetrievalServiceMock.Object,
            _ocrServiceMock.Object,
            configuration

        );
    }

    [Fact]
    public async Task GetPdfAsync_WhenAPdfIsAlreadyInCache_WillReturnCachedPdf()
    {
        // Arrange
        var cachedPdfStream = new MemoryStream(_fixture.Create<byte[]>()) as Stream;

        _cacheServiceMock
            .Setup(x => x.TryGetPdfAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync((true, cachedPdfStream));

        var expectedResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(cachedPdfStream, true))
            .Returns(expectedResult);

        // Act
        var result = await _pdfArtefactService.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetPdfAsync_WhenAPdfIsNotAlreadyInCacheAndConversionFails_WillReturnFailedResult()
    {
        // Arrange
        _cacheServiceMock
            .Setup(x => x.TryGetPdfAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync((false, null));

        _pdfRetrievalServiceMock
            .Setup(x => x.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId))
            .ReturnsAsync(new DocumentRetrievalResult { Status = PdfConversionStatus.DocumentTypeUnsupported });

        var expectedResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateFailedResult<Stream>(PdfConversionStatus.DocumentTypeUnsupported, It.IsAny<int?>()))
            .Returns(expectedResult);

        // Act
        var result = await _pdfArtefactService.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetPdfAsync_WhenAPdfIsNotAlreadyInCacheAndConversionSucceeds_WillReturnOkResult()
    {
        // Arrange
        var pdfStream = new MemoryStream(_fixture.Create<byte[]>()) as Stream;

        _cacheServiceMock
            .SetupSequence(x => x.TryGetPdfAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync((false, null))
            .ReturnsAsync((true, pdfStream));

        _cacheServiceMock
            .Setup(x => x.UploadPdfAsync(_caseId, _documentId, _versionId, false, It.IsAny<Stream>(), It.IsAny<double>()))
            .Returns(Task.CompletedTask);

        var fakeOcrOperationId = Guid.NewGuid();
        var fakeOcrResult = new OcrOperationResult
        {
            IsSuccess = true,
            AnalyzeResults = new AnalyzeResults()
        };

        _ocrServiceMock
            .Setup(x => x.InitiateOperationAsync(It.IsAny<Stream>(), It.IsAny<Guid>()))
            .ReturnsAsync(fakeOcrOperationId);

        _ocrServiceMock
            .Setup(x => x.GetOperationResultsAsync(fakeOcrOperationId, It.IsAny<Guid>()))
            .ReturnsAsync(fakeOcrResult);

        _cacheServiceMock
            .Setup(x => x.UploadJsonObjectAsync(
                _caseId, _documentId, _versionId, BlobType.Ocr, fakeOcrResult.AnalyzeResults))
            .Returns(Task.CompletedTask);

        _pdfRetrievalServiceMock
            .Setup(x => x.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId))
            .ReturnsAsync(new DocumentRetrievalResult { Status = PdfConversionStatus.DocumentConverted, PdfStream = pdfStream });

        var expectedResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(pdfStream, false))
            .Returns(expectedResult);

        // Act
        var result = await _pdfArtefactService.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);
        _cacheServiceMock.Verify(x => x.UploadPdfAsync(_caseId, _documentId, _versionId, false, It.IsAny<Stream>(), It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public async Task GetPdfAsync_WhenCachedPdfExceedsSizeLimit_ShouldReturnLargeFileFlag()
    {
        // Arrange
        var largeStream = new MemoryStream(new byte[20 * 1024 * 1024]); // 20 MB

        _cacheServiceMock
            .Setup(x => x.TryGetPdfAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync((true, (Stream)largeStream));

        _cacheServiceMock
            .Setup(x => x.GetPdfSizeFromMetadataAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync(20); // MB (above limit of 10)

        var expectedResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkResultWithLargeFileFlag(It.IsAny<Stream>(), true, true))
            .Returns(expectedResult);

        // Act
        var result = await _pdfArtefactService.GetPdfAsync(
            _cmsAuthValues,
            _correlationId,
            _urn,
            _caseId,
            _documentId,
            _versionId,
            false);

        // Assert
        result.Should().Be(expectedResult);

        _artefactServiceResponseFactoryMock.Verify(
            x => x.CreateOkResultWithLargeFileFlag(It.IsAny<Stream>(), true, true),
            Times.Once);
    }

    [Fact]
    public async Task GetPdfAsync_WhenCachedPdfWithinLimit_ShouldReturnNormalResult()
    {
        // Arrange
        var smallStream = new MemoryStream(new byte[1 * 1024 * 1024]); // 1 MB

        _cacheServiceMock
            .Setup(x => x.TryGetPdfAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync((true, (Stream)smallStream));

        _cacheServiceMock
            .Setup(x => x.GetPdfSizeFromMetadataAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync(1); // below limit

        var expectedResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkfResult(It.IsAny<Stream>(), true))
            .Returns(expectedResult);

        // Act
        var result = await _pdfArtefactService.GetPdfAsync(
            _cmsAuthValues,
            _correlationId,
            _urn,
            _caseId,
            _documentId,
            _versionId,
            false);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetPdfAsync_WhenNewPdfExceedsSizeLimit_ShouldReturnLargeFileFlag()
    {
        // Arrange
        var largeBytes = new byte[20 * 1024 * 1024];
        var stream = new MemoryStream(largeBytes);

        _cacheServiceMock
            .SetupSequence(x => x.TryGetPdfAsync(_caseId, _documentId, _versionId, false))
            .ReturnsAsync((false, null))
            .ReturnsAsync((true, (Stream)stream));

        _pdfRetrievalServiceMock
            .Setup(x => x.GetPdfStreamAsync(
                _cmsAuthValues, _correlationId, _urn,
                _caseId, _documentId, _versionId))
            .ReturnsAsync(new DocumentRetrievalResult
            {
                Status = PdfConversionStatus.DocumentConverted,
                PdfStream = new MemoryStream(largeBytes)
            });

        _cacheServiceMock
            .Setup(x => x.UploadPdfAsync(
                _caseId, _documentId, _versionId,
                false, It.IsAny<Stream>(), It.IsAny<double>()))
            .Returns(Task.CompletedTask);

        var fakeOcrOperationId = Guid.NewGuid();

        _ocrServiceMock
            .Setup(x => x.InitiateOperationAsync(It.IsAny<Stream>(), It.IsAny<Guid>()))
            .ReturnsAsync(fakeOcrOperationId);

        _ocrServiceMock
            .Setup(x => x.GetOperationResultsAsync(fakeOcrOperationId, It.IsAny<Guid>()))
            .ReturnsAsync(new OcrOperationResult
            {
                IsSuccess = true,
                AnalyzeResults = new AnalyzeResults()
            });

        _cacheServiceMock
            .Setup(x => x.UploadJsonObjectAsync(
                _caseId, _documentId, _versionId,
                BlobType.Ocr, It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var expectedResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateOkResultWithLargeFileFlag(It.IsAny<Stream>(), false, true))
            .Returns(expectedResult);

        // Act
        var result = await _pdfArtefactService.GetPdfAsync(
            _cmsAuthValues, _correlationId, _urn,
            _caseId, _documentId, _versionId, false);

        // Assert
        result.Should().Be(expectedResult);

        _artefactServiceResponseFactoryMock.Verify(
            x => x.CreateOkResultWithLargeFileFlag(It.IsAny<Stream>(), false, true),
            Times.Once);
    }
}
