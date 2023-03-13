using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Clients;
using Common.Clients.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PolarisGateway.Tests.Clients.PolarisPipeline
{
    public class BlobStorageClientTests
	{
        private readonly string _blobName;
        private readonly Guid _correlationId;

        private readonly Mock<Response<bool>> _mockBlobContainerExistsResponse;
        private readonly Mock<Response<bool>> _mockBlobClientExistsResponse;

        private readonly IBlobStorageClient _blobStorageClient;

		public BlobStorageClientTests()
		{
            var fixture = new Fixture();
			var blobContainerName = fixture.Create<string>();
			_blobName = fixture.Create<string>();
			_correlationId = fixture.Create<Guid>();
			var binaryData = new BinaryData(Array.Empty<byte>());

			var mockBlobServiceClient = new Mock<BlobServiceClient>();
			var mockBlobContainerClient = new Mock<BlobContainerClient>();
			var mockBlobClient = new Mock<BlobClient>();
			var mockBlobDownloadResponse = new Mock<Response<BlobDownloadResult>>();
			var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(binaryData);

			mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerName))
				.Returns(mockBlobContainerClient.Object);

			_mockBlobContainerExistsResponse = new Mock<Response<bool>>();
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(true);
			mockBlobContainerClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_mockBlobContainerExistsResponse.Object);
			mockBlobContainerClient.Setup(client => client.GetBlobClient(_blobName)).Returns(mockBlobClient.Object);

			_mockBlobClientExistsResponse = new Mock<Response<bool>>();
			_mockBlobClientExistsResponse.Setup(response => response.Value).Returns(true);
			mockBlobClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_mockBlobClientExistsResponse.Object);
			mockBlobClient.Setup(client => client.DownloadContentAsync()).ReturnsAsync(mockBlobDownloadResponse.Object);

			mockBlobDownloadResponse.Setup(response => response.Value).Returns(blobDownloadResult);

			var mockLogger = new Mock<ILogger<BlobStorageClient>>();

			_blobStorageClient = new BlobStorageClient(mockBlobServiceClient.Object, blobContainerName, mockLogger.Object);
		}

		[Fact]
		public async Task GetDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
        {
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageClient.GetDocumentAsync(_blobName, _correlationId));
		}

		[Fact]
		public async Task GetDocumentAsync_ReturnsNullWhenBlobDoesNotExist()
		{
			_mockBlobClientExistsResponse.Setup(response => response.Value).Returns(false);

			var document = await _blobStorageClient.GetDocumentAsync(_blobName, _correlationId);

			document.Should().BeNull();
		}

		[Fact]
		public async Task GetDocumentAsync_ReturnsBlobStream()
		{
			var document = await _blobStorageClient.GetDocumentAsync(_blobName, _correlationId);

			document.Should().NotBeNull();
		}
	}
}

