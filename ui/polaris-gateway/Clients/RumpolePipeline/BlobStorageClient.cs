using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using RumpoleGateway.Domain.Logging;

namespace RumpoleGateway.Clients.RumpolePipeline
{
	public class BlobStorageClient : IBlobStorageClient
	{
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobServiceContainerName;
        private readonly ILogger<BlobStorageClient> _logger;

        public BlobStorageClient(BlobServiceClient blobServiceClient, string blobServiceContainerName, ILogger<BlobStorageClient> logger)
        {
            _blobServiceClient = blobServiceClient;
            _blobServiceContainerName = blobServiceContainerName;
            _logger = logger;
        }

        public async Task<Stream> GetDocumentAsync(string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"Loading document for '{blobName}'");
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);

            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
                return null;
            
            var blob = await blobClient.DownloadContentAsync();

            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return blob.Value.Content.ToStream();
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(UploadDocumentAsync), $"Uploading document to Polaris blob storage, blob name: '{blobName}'");
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);

            if (!await blobContainerClient.ExistsAsync())
            {
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            }

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(stream, true);
            _logger.LogMethodExit(correlationId, nameof(UploadDocumentAsync), string.Empty);
        }
    }
}

