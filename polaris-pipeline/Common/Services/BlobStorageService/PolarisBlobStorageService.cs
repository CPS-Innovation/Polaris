using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Constants;
using Common.Domain.BlobStorage;
using Common.Domain.Extensions;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Common.Services.BlobStorageService
{
    public class  PolarisBlobStorageService : IPolarisBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobServiceContainerName;
        private readonly ILogger<PolarisBlobStorageService> _logger;

        public PolarisBlobStorageService(BlobServiceClient blobServiceClient, string blobServiceContainerName, ILogger<PolarisBlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _blobServiceContainerName = blobServiceContainerName;
            _logger = logger;
        }
        
        public async Task<bool> DocumentExistsAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            return await blobClient.ExistsAsync();
        }

        public async Task<List<BlobSearchResult>> FindBlobsByPrefixAsync(string blobPrefix, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(FindBlobsByPrefixAsync), blobPrefix);
            var result = new List<BlobSearchResult>();
            
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            await foreach (var blobItem in blobContainerClient.GetBlobsAsync (BlobTraits.Metadata, BlobStates.None, blobPrefix))
            {
                blobItem.Metadata.TryGetValue(DocumentTags.VersionId, out var blobVersionAsString);
                var convResult = long.TryParse(blobVersionAsString, out var versionId);
                result.Add(new BlobSearchResult
                {
                    BlobName = blobItem.Name,
                    VersionId = convResult ? versionId : 1
                });
            }

            return result;
        }

        public async Task<Stream> GetDocumentAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            if (!await blobClient.ExistsAsync())
                return null;
            
            var blob = await blobClient.DownloadContentAsync();

            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return blob.Value.Content.ToStream();
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string versionId, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(UploadDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            await blobClient.UploadAsync(stream, true);
            stream.Close();

            var metadata = new Dictionary<string, string>
            {
                {DocumentTags.CaseId, caseId},
                {DocumentTags.DocumentId, documentId},
                {DocumentTags.VersionId, string.IsNullOrWhiteSpace(versionId) ? "1" : versionId}
            };

            await blobClient.SetMetadataAsync(metadata);

            _logger.LogMethodExit(correlationId, nameof(UploadDocumentAsync), string.Empty);
        }

        public async Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(RemoveDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            try
            {
                var deleteResult = await blobClient.DeleteIfExistsAsync();
                _logger.LogMethodFlow(correlationId, nameof(RemoveDocumentAsync), deleteResult ? $"Blob '{decodedBlobName}' deleted successfully from '{_blobServiceContainerName}'" 
                    : $"Blob '{decodedBlobName}' deleted unsuccessfully from '{_blobServiceContainerName}'");
                return true;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode != (int) HttpStatusCode.NotFound) throw;

                if (e.RequestInformation.ExtendedErrorInformation != null && e.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound)
                    return true; //nothing to remove, probably because it is the first time for the case or the blob storage has undergone lifecycle management
                
                throw;
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(RemoveDocumentAsync), string.Empty);
            }
        }
    }
}