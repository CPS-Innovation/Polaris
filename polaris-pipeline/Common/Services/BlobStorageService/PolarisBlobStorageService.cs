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
using Common.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Common.Services.BlobStorageService
{
    public class PolarisBlobStorageService : IPolarisBlobStorageService
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

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            return await blobClient.ExistsAsync();
        }

        public async Task<List<BlobSearchResult>> FindBlobsByPrefixAsync(string blobPrefix, Guid correlationId)
        {
            var result = new List<BlobSearchResult>();

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            await foreach (var blobItem in blobContainerClient.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, blobPrefix))
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

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            if (!await blobClient.ExistsAsync())
                return null;

            // We could use `DownloadStreamingAsync` as per https://github.com/Azure/azure-sdk-for-net/issues/22022#issuecomment-870054035
            //  as we are in Azure calling Azure so streaming should be no problem without having to do chunking.
            // However https://github.com/Azure/azure-sdk-for-net/issues/38342#issue-1864138162 suggests that we could better use `OpenReadAsync`.
            return await blobClient.OpenReadAsync();
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName, string caseId, PolarisDocumentId polarisDocumentId, string versionId, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            await blobClient.UploadAsync(stream, true);
            stream.Close();

            var metadata = new Dictionary<string, string>
            {
                {DocumentTags.CaseId, caseId},
                {DocumentTags.DocumentId, polarisDocumentId.Value},
                {DocumentTags.VersionId, string.IsNullOrWhiteSpace(versionId) ? "1" : versionId}
            };

            await blobClient.SetMetadataAsync(metadata);
        }

        public async Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            try
            {
                var deleteResult = await blobClient.DeleteIfExistsAsync();
                return true;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode != (int)HttpStatusCode.NotFound) throw;

                if (e.RequestInformation.ExtendedErrorInformation != null && e.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound)
                    return true; //nothing to remove, probably because it is the first time for the case or the blob storage has undergone lifecycle management

                throw;
            }
        }

        public async Task DeleteBlobsByCaseAsync(string caseId, Guid correlationId)
        {
            var blobCount = 0;
            var targetFolderPath = $"{caseId}/pdfs";
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            await foreach (var blobItem in blobContainerClient.GetBlobsAsync(prefix: targetFolderPath))
            {
                var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                var deleteResult = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

                if (deleteResult)
                    blobCount++;
            }
        }
    }
}