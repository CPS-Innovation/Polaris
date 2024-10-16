using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Wrappers;
using Microsoft.WindowsAzure.Storage;

namespace Common.Services.BlobStorageService
{
    public class PolarisBlobStorageService : IPolarisBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobServiceContainerName;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public PolarisBlobStorageService(BlobServiceClient blobServiceClient, string blobServiceContainerName, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobServiceClient = blobServiceClient;
            _blobServiceContainerName = blobServiceContainerName;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<Stream> GetBlobOrThrowAsync(string blobName)
        {
            var stream = await GetBlobAsync(blobName, null);
            return stream ?? throw new StorageException("Blob not found");
        }

        public async Task<Stream> GetBlobAsync(string blobName)
        {
            return await GetBlobAsync(blobName, null);
        }

        public async Task<Stream> GetBlobAsync(string blobName, IDictionary<string, string> mustMatchMetadata)
        {
            var decodedBlobName = UrlDecodeString(blobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            if (mustMatchMetadata != null)
            {
                var storedMetaData = (await blobClient.GetPropertiesAsync()).Value.Metadata;
                var metadataMatch = mustMatchMetadata.All(kvp => storedMetaData.ContainsKey(kvp.Key) && storedMetaData[kvp.Key] == kvp.Value);
                if (!metadataMatch)
                {
                    return null;
                }
            }

            // We could use `DownloadStreamingAsync` as per https://github.com/Azure/azure-sdk-for-net/issues/22022#issuecomment-870054035
            //  as we are in Azure calling Azure so streaming should be no problem without having to do chunking.
            // However https://github.com/Azure/azure-sdk-for-net/issues/38342#issue-1864138162 suggests that we could better use `OpenReadAsync`.
            //  Azurite seems to have a problem with `OpenReadAsync` so we will use `DownloadStreamingAsync` for now.
            var result = await blobClient.DownloadStreamingAsync();
            return result.Value.Content;
        }

        public async Task<T> GetObjectAsync<T>(string blobName)
        {
            using var stream = await GetBlobAsync(blobName);
            if (stream == null)
            {
                return default;
            }
            using var streamReader = new StreamReader(stream);
            return _jsonConvertWrapper.DeserializeObject<T>(await streamReader.ReadToEndAsync());
        }

        public async Task UploadBlobAsync(Stream stream, string blobName)
        {
            await UploadBlobInternal(stream, blobName);
        }
        public async Task UploadBlobAsync(Stream stream, string blobName, IDictionary<string, string> metadata)
        {
            var blobClient = await UploadBlobInternal(stream, blobName);
            await blobClient.SetMetadataAsync(metadata);
        }

        public async Task UploadObjectAsync<T>(T obj, string blobName)
        {
            var objectString = _jsonConvertWrapper.SerializeObject(obj);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(objectString ?? ""));
            await UploadBlobAsync(stream, blobName);
        }

        public async Task DeleteBlobsByCaseAsync(int caseId)
        {
            var blobCount = 0;
            var targetFolderPath = caseId.ToString();
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

        private async Task<BlobClient> UploadBlobInternal(Stream stream, string blobName)
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

        private static string UrlDecodeString(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.UnescapeDataString(value);
        }
    }
}