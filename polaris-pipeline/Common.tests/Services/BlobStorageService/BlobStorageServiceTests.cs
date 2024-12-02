using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Services.BlobStorage;
using Common.Wrappers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Common.tests.Services.BlobStorageService
{
    public class BlobStorageServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Stream _stream;
        private readonly string _blobName;

        private readonly Mock<Response<bool>> _mockBlobContainerExistsResponse;
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private readonly Mock<Response> _responseMock;
        private readonly Mock<IJsonConvertWrapper> _jsonConvertWrapper;
        private readonly IBlobStorageService _blobStorageService;

        public BlobStorageServiceTests()
        {
            _fixture = new Fixture();
            var blobContainerName = _fixture.Create<string>();
            _stream = new MemoryStream();
            _blobName = _fixture.Create<string>();

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

            _jsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _blobStorageService = new Common.Services.BlobStorage.BlobStorageService(mockBlobServiceClient.Object, blobContainerName, _jsonConvertWrapper.Object);
        }

        #region GetDocumentAsync

        [Fact]
        public async Task GetDocumentAsync_ThrowsRequestFailedException_WhenBlobContainerDoesNotExist()
        {
            _mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

            await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.GetBlob(_blobName));
        }

        [Fact]
        public async Task GetDocumentAsync_ReturnsNull_WhenBlobClientCannotBeFound()
        {
            _mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(false, _responseMock.Object));

            await Assert.ThrowsAsync<StorageException>(() => _blobStorageService.GetBlob(_blobName));
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

            var result = await _blobStorageService.GetBlob(_blobName);

            result.Should().BeSameAs(_stream);
        }

        #endregion

        #region UploadDocumentAsync

        [Fact]
        public async Task UploadDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
        {
            _mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

            await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.UploadBlobAsync(_stream, _blobName));
        }

        #endregion
    }

    internal class TestClass
    {

    }
}

