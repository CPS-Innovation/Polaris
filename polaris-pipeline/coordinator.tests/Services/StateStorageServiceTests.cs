using Common.Services.BlobStorage;
using coordinator.Services;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Common.Configuration;
using Common.Dto.Response;
using coordinator.Domain;
using Xunit;

namespace coordinator.tests.Services;

public class StateStorageServiceTests
{
    private readonly Mock<IPolarisBlobStorageService> _polarisBlobStorageServiceMock;
    private readonly StateStorageService _stateStorageService;
    public StateStorageServiceTests()
    {
        _polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        var containerName = "containerName";
        var blobStorageServiceFactoryMock = new Mock<Func<string, IPolarisBlobStorageService>>();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(s => s[StorageKeys.BlobServiceContainerNameDocuments]).Returns(containerName);
        blobStorageServiceFactoryMock.Setup(s => s(containerName)).Returns(_polarisBlobStorageServiceMock.Object);
        _stateStorageService = new StateStorageService(blobStorageServiceFactoryMock.Object, configurationMock.Object);
    }

    #region GetBulkRedactionSearchStateAsync

    [Fact]
    public async Task GetBulkRedactionSearchStateAsync_BlobStorageReturnsNull_ShouldReturnEmptyObjectNotStarted()
    {
        //arrange
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var searchText = "SearchText";
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<BulkRedactionSearchEntityState>(It.IsAny<BlobIdType>())).ReturnsAsync((BulkRedactionSearchEntityState)null);

        //act
        var result = await _stateStorageService.GetBulkRedactionSearchStateAsync(caseId, documentId, versionId, searchText);

        //assert
        Assert.Equal(BulkRedactionSearchStatus.NotStarted, result.Status);
        Assert.Equal(searchText, result.SearchTerm);
        Assert.Equal(caseId, result.CaseId);
        Assert.Equal(versionId, result.VersionId);
        Assert.Equal(documentId, result.DocumentId);
    }

    [Fact]
    public async Task GetBulkRedactionSearchStateAsync_ShouldReturnResultFromBlobStorage()
    {
        //arrange
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var searchText = "SearchText";
        var bulkRedactionSearchEntityState = new BulkRedactionSearchEntityState();
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<BulkRedactionSearchEntityState>(It.IsAny<BlobIdType>())).ReturnsAsync(bulkRedactionSearchEntityState);

        //act
        var result = await _stateStorageService.GetBulkRedactionSearchStateAsync(caseId, documentId, versionId, searchText);

        //assert
        Assert.Same(bulkRedactionSearchEntityState, result);
    }

    #endregion

    #region UpdateBulkRedactionSearchStateAsync

    [Fact]
    public async Task UpdateBulkRedactionSearchStateAsync_ShouldReturnTrue()
    {
        //arrange
        var bulkRedactionSearchEntityState = new BulkRedactionSearchEntityState();

        //act
        var result = await _stateStorageService.UpdateBulkRedactionSearchStateAsync(bulkRedactionSearchEntityState);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(bulkRedactionSearchEntityState, It.IsAny<BlobIdType>()),Times.Once);
        Assert.True(result);
    }


    #endregion
}