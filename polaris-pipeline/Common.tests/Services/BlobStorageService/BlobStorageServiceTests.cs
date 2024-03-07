using System.Net;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Constants;
using Common.Services.BlobStorageService;
using Common.Services.BlobStorageService.Contracts;
using Common.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Moq;
using Xunit;

namespace Common.tests.Services.BlobStorageService
{
    public class BlobStorageServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Stream _stream;
        private readonly string _blobName;
        private readonly Guid _correlationId;
        private readonly long _caseId;
        private readonly PolarisDocumentId _polarisDocumentId;
        private readonly long _versionId;

        private readonly Mock<Response<bool>> _mockBlobContainerExistsResponse;
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private readonly Mock<Response> _responseMock;

        private readonly IPolarisBlobStorageService _blobStorageService;

        public BlobStorageServiceTests()
        {
            _fixture = new Fixture();
            var blobContainerName = _fixture.Create<string>();
            _stream = new MemoryStream();
            _blobName = _fixture.Create<string>();
            _correlationId = _fixture.Create<Guid>();
            _caseId = _fixture.Create<long>();
            _polarisDocumentId = _fixture.Create<PolarisDocumentId>();
            _versionId = _fixture.Create<long>();

            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();
            _responseMock = new Mock<Response>();

            mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerName))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerExistsResponse = new Mock<Response<bool>>();
            _mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(true);
            _mockBlobContainerClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockBlobContainerExistsResponse.Object);
            _mockBlobContainerClient.Setup(client => client.GetBlobClient(_blobName)).Returns(_mockBlobClient.Object);
            var mockLogger = new Mock<ILogger<Common.Services.BlobStorageService.PolarisBlobStorageService>>();

            _blobStorageService = new Common.Services.BlobStorageService.PolarisBlobStorageService(mockBlobServiceClient.Object, blobContainerName, mockLogger.Object);
        }

        #region GetDocumentAsync

        [Fact]
        public async Task GetDocumentAsync_ThrowsRequestFailedException_WhenBlobContainerDoesNotExist()
        {
            _mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

            await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.GetDocumentAsync(_blobName, _correlationId));
        }

        [Fact]
        public async Task GetDocumentAsync_ReturnsNull_WhenBlobClientCannotBeFound()
        {
            _mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(false, _responseMock.Object));

            await Assert.ThrowsAsync<StorageException>(() => _blobStorageService.GetDocumentAsync(_blobName, _correlationId));
        }

        [Fact(Skip = "Not possible to adequately mock BlobDownloadStreamingResult")]
        public async Task GetDocumentAsync_ReturnsTheBlobStream_WhenBlobClientIsFound()
        {

            _mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(true, _responseMock.Object));

            var mockBlobDownloadStreamingResult = new Mock<BlobDownloadStreamingResult>();
            mockBlobDownloadStreamingResult.SetupGet(result => result.Content).Returns(_stream);

            var mockResponseOfBlobDownloadStreamingResult = new Mock<Response<BlobDownloadStreamingResult>>();
            mockResponseOfBlobDownloadStreamingResult
                .Setup(response => response.Value)
                .Returns(mockBlobDownloadStreamingResult.Object);

            _mockBlobClient.Setup(s => s.DownloadStreamingAsync(It.IsAny<HttpRange>(), It.IsAny<BlobRequestConditions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponseOfBlobDownloadStreamingResult.Object);

            var result = await _blobStorageService.GetDocumentAsync(_blobName, _correlationId);

            result.Should().BeSameAs(_stream);
        }

        #endregion

        #region UploadDocumentAsync

        [Fact]
        public async Task UploadDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
        {
            _mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

            await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.UploadDocumentAsync(_stream, _blobName, _caseId.ToString(), _polarisDocumentId,
                _versionId.ToString(), _correlationId));
        }

        [Fact]
        public async Task UploadDocumentAsync_UploadsDocument()
        {
            await _blobStorageService.UploadDocumentAsync(_stream, _blobName, _caseId.ToString(), _polarisDocumentId, _versionId.ToString(), _correlationId);

            _mockBlobClient.Verify(client => client.UploadAsync(_stream, true, It.IsAny<CancellationToken>()));
        }

        #endregion
    }
}

