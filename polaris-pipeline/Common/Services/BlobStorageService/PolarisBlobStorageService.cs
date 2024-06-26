﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.ValueObjects;
using Microsoft.WindowsAzure.Storage;

namespace Common.Services.BlobStorageService
{
    public class PolarisBlobStorageService : IPolarisBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobServiceContainerName;

        public PolarisBlobStorageService(BlobServiceClient blobServiceClient, string blobServiceContainerName)
        {
            _blobServiceClient = blobServiceClient;
            _blobServiceContainerName = blobServiceContainerName;
        }

        public async Task<Stream> GetDocumentAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = UrlDecodeString(blobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            if (!await blobClient.ExistsAsync())
            {
                throw new StorageException($"Blob '{decodedBlobName}' does not exist");
            }

            // We could use `DownloadStreamingAsync` as per https://github.com/Azure/azure-sdk-for-net/issues/22022#issuecomment-870054035
            //  as we are in Azure calling Azure so streaming should be no problem without having to do chunking.
            // However https://github.com/Azure/azure-sdk-for-net/issues/38342#issue-1864138162 suggests that we could better use `OpenReadAsync`.
            //  Azurite seems to have a problem with `OpenReadAsync` so we will use `DownloadStreamingAsync` for now.
            var result = await blobClient.DownloadStreamingAsync();
            return result.Value.Content;
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName)
        {
            await UploadDocumentInternal(stream, blobName);
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName, string caseId, PolarisDocumentId polarisDocumentId, string versionId, Guid correlationId)
        {
            var blobClient = await UploadDocumentInternal(stream, blobName);

            var metadata = new Dictionary<string, string>
            {
                {DocumentTags.CaseId, caseId},
                {DocumentTags.DocumentId, polarisDocumentId.Value},
                {DocumentTags.VersionId, string.IsNullOrWhiteSpace(versionId) ? "1" : versionId}
            };

            await blobClient.SetMetadataAsync(metadata);
        }

        public async Task DeleteBlobsByCaseAsync(string caseId)
        {
            var blobCount = 0;
            var targetFolderPath = caseId;
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

        private static string UrlDecodeString(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.UnescapeDataString(value);
        }

        private async Task<BlobClient> UploadDocumentInternal(Stream stream, string blobName)
        {
            var decodedBlobName = UrlDecodeString(blobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            await blobClient.UploadAsync(stream, true);
            stream.Close();

            return blobClient;
        }
    }
}