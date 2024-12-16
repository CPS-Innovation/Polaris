using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using FluentAssertions;
using Moq;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
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

        _pdfArtefactService = new PdfArtefactService(
            _cacheServiceMock.Object,
            _artefactServiceResponseFactoryMock.Object,
            _pdfRetrievalServiceMock.Object
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
            .Setup(x => x.CreateOkfResult(cachedPdfStream, false))
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
            .Setup(x => x.CreateFailedResult<Stream>(PdfConversionStatus.DocumentTypeUnsupported))
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
            .Setup(x => x.UploadPdfAsync(_caseId, _documentId, _versionId, false, pdfStream))
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
        _cacheServiceMock.Verify(x => x.UploadPdfAsync(_caseId, _documentId, _versionId, false, pdfStream), Times.Once);
    }
}