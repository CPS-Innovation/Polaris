using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Common.Configuration;
using Common.Services.BlobStorage;
using Common.Services.BlobStorage.Factories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PolarisGateway.Services.Artefact;
using Xunit;

namespace PolarisGateway.Tests.Services.Artefact;

public class CacheServiceTests
{
    private readonly Fixture _fixture;

    private readonly int _caseId;
    private readonly long _versionId;
    private readonly string _documentId;
    private readonly bool _isOcrProcessed;

    private readonly Mock<IPolarisBlobStorageService> _polarisBlobStorageServiceMock;
    private readonly BlobIdType _blobIdType;

    private readonly CacheService _cacheService;
    public CacheServiceTests()
    {
        _fixture = new Fixture();

        _caseId = _fixture.Create<int>();
        _versionId = _fixture.Create<long>();
        _documentId = _fixture.Create<string>();
        _isOcrProcessed = _fixture.Create<bool>();

        _polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        var storageKey = _fixture.Create<string>();
        IPolarisBlobStorageService blobStorageFactory(string key) => key == storageKey
            ? _polarisBlobStorageServiceMock.Object
            : throw new ArgumentException();

        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(x => x[StorageKeys.BlobServiceContainerNameDocuments])
            .Returns(storageKey);


        _blobIdType = _fixture.Create<BlobIdType>();
        var blobTypeIdFactoryMock = new Mock<IBlobTypeIdFactory>();
        blobTypeIdFactoryMock
                .Setup(x => x.CreateBlobId(_caseId, _documentId, _versionId, BlobType.Pdf))
                .Returns(_blobIdType);

        _cacheService = new CacheService(
            blobStorageFactory,
            configurationMock.Object,
            blobTypeIdFactoryMock.Object
        );
    }

    [Fact]
    public async Task TryGetPdfAsync_WhenAPdfIsAlreadyInCache_WillReturnCachedPdf()
    {
        // Arrange

        var cachedPdfStream = new MemoryStream(_fixture.Create<byte[]>());

        _polarisBlobStorageServiceMock
            .Setup(x => x.TryGetBlobAsync(_blobIdType, _isOcrProcessed))
            .ReturnsAsync(cachedPdfStream);

        // Act
        var (isCached, result) = await _cacheService.TryGetPdfAsync(_caseId, _documentId, _versionId, _isOcrProcessed);

        // Assert
        isCached.Should().BeTrue();
        result.Should().BeSameAs(cachedPdfStream);
    }

    [Fact]
    public async Task TryGetPdfAsync_WhenAPdfIsNotAlreadyInCache_WillReturnCachedPdf()
    {
        // Arrange
        var cachedPdfStream = new MemoryStream(_fixture.Create<byte[]>());

        _polarisBlobStorageServiceMock
            .Setup(x => x.TryGetBlobAsync(_blobIdType, _isOcrProcessed));


        // Act
        var (isCached, result) = await _cacheService.TryGetPdfAsync(_caseId, _documentId, _versionId, _isOcrProcessed);

        // Assert
        isCached.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadPdfAsync_WhenUploadingPdf_WillUploadPdf()
    {
        // Arrange
        var pdfStream = new MemoryStream(_fixture.Create<byte[]>());

        // Act
        await _cacheService.UploadPdfAsync(_caseId, _documentId, _versionId, _isOcrProcessed, pdfStream);

        // Assert
        _polarisBlobStorageServiceMock
            .Verify(x => x.UploadBlobAsync(pdfStream, _blobIdType, _isOcrProcessed), Times.Once);
    }

    [Fact]
    public async Task TryGetJsonObjectAsync_WhenJsonObjectIsAlreadyInCache_WillReturnCachedJsonObject()
    {
        // Arrange
        var cachedJsonObject = _fixture.Create<object>();

        _polarisBlobStorageServiceMock
            .Setup(x => x.TryGetObjectAsync<object>(_blobIdType))
            .ReturnsAsync(cachedJsonObject);

        // Act
        var (isCached, result) = await _cacheService.TryGetJsonObjectAsync<object>(_caseId, _documentId, _versionId, BlobType.Pdf);

        // Assert
        isCached.Should().BeTrue();
        result.Should().BeSameAs(cachedJsonObject);
    }

    [Fact]
    public async Task TryGetJsonObjectAsync_WhenJsonObjectIsNotAlreadyInCache_WillReturnCachedJsonObject()
    {
        // Arrange
        _polarisBlobStorageServiceMock
            .Setup(x => x.TryGetObjectAsync<object>(_blobIdType));

        // Act
        var (isCached, result) = await _cacheService.TryGetJsonObjectAsync<object>(_caseId, _documentId, _versionId, BlobType.Pdf);

        // Assert
        isCached.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadJsonObjectAsync_WhenUploadingJsonObject_WillUploadJsonObject()
    {
        // Arrange
        var jsonObject = _fixture.Create<object>();

        // Act
        await _cacheService.UploadJsonObjectAsync(_caseId, _documentId, _versionId, BlobType.Pdf, jsonObject);

        // Assert
        _polarisBlobStorageServiceMock
            .Verify(x => x.UploadObjectAsync(jsonObject, _blobIdType), Times.Once);
    }

}