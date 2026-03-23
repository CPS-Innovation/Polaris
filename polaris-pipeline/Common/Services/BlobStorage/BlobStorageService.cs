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

namespace Common.Services.BlobStorage;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _blobServiceContainerName;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public BlobStorageService(BlobServiceClient blobServiceClient, string blobServiceContainerName, IJsonConvertWrapper jsonConvertWrapper)
    {
        _blobServiceClient = blobServiceClient;
        _blobServiceContainerName = blobServiceContainerName;
        _jsonConvertWrapper = jsonConvertWrapper;
    }

    public async Task<T> TryGetObjectAsync<T>(string blobName)
    {
        await using var stream = await TryGetBlobAsync(blobName);
        if (stream == null)
        {
            return default;
        }
        using var streamReader = new StreamReader(stream);
        return _jsonConvertWrapper.DeserializeObject<T>(await streamReader.ReadToEndAsync());
    }

    public async Task<Stream> GetBlob(string blobName)
    {
        var stream = await TryGetBlobAsync(blobName);
        return stream ?? throw new RequestFailedException("Blob not found");
    }


    public async Task<Stream> TryGetBlobAsync(string blobName, IDictionary<string, string> mustMatchMetadata = null)
    {
        return await TryGetBlobAsync(blobName, mustMatchMetadata, System.Threading.CancellationToken.None);
    }

    public async Task UploadBlobAsync(Stream stream, string blobName, IDictionary<string, string> metadata = null)
    {
        await UploadBlobAsync(stream, blobName, metadata, System.Threading.CancellationToken.None);
    }

    public async Task<bool> UploadObjectAsync<T>(T obj, string blobName)
    {
        var objectString = _jsonConvertWrapper.SerializeObject(obj);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(objectString ?? ""));
        await UploadBlobInternal(stream, blobName, System.Threading.CancellationToken.None);

        return true;
    }

    public async Task DeleteBlobsByPrefix(string prefix)
    {
        await DeleteBlobsByPrefix(prefix, System.Threading.CancellationToken.None);
    }

    public async Task<bool> BlobExistsAsync(string blobName, IDictionary<string, string> mustMatchMetadata = null)
    {
        return await BlobExistsAsync(blobName, mustMatchMetadata, System.Threading.CancellationToken.None);
    }

    private async Task<BlobContainerClient> GetBlobContainerClientOrThrow()
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
        if (!await blobContainerClient.ExistsAsync())
        {
            throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
        }

        return blobContainerClient;
    }

    private async Task<BlobClient> UploadBlobInternal(Stream stream, string blobName, System.Threading.CancellationToken cancellationToken)
    {
        var blobContainerClient = await GetBlobContainerClientOrThrow();
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, true, cancellationToken);
        stream.Close();

        return blobClient;
    }

    private static async Task<bool> IsMetadataMatch(BlobClient blobClient, IDictionary<string, string> mustMatchMetadata, System.Threading.CancellationToken cancellationToken)
    {
        var storedMetaData = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken)).Value.Metadata;
        return mustMatchMetadata.All(kvp => storedMetaData.ContainsKey(kvp.Key) && storedMetaData[kvp.Key] == kvp.Value);
    }

    public async Task<Stream> TryGetBlobAsync(string blobName, IDictionary<string, string> mustMatchMetadata, System.Threading.CancellationToken cancellationToken)
    {
        var blobContainerClient = await GetBlobContainerClientOrThrow();
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        if (mustMatchMetadata != null)
        {
            var metadataMatch = await IsMetadataMatch(blobClient, mustMatchMetadata, cancellationToken);
            if (!metadataMatch)
            {
                return null;
            }
        }

        var result = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return result.Value.Content;
    }

    public async Task UploadBlobAsync(Stream stream, string blobName, IDictionary<string, string> metadata, System.Threading.CancellationToken cancellationToken)
    {
        var blobClient = await UploadBlobInternal(stream, blobName, cancellationToken);
        await blobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
    }

    public async Task UploadBlobAsync(Stream stream, string blobName, System.Threading.CancellationToken cancellationToken)
    {
        await UploadBlobInternal(stream, blobName, cancellationToken);
    }

    public async Task<T> TryGetObjectAsync<T>(string blobName, System.Threading.CancellationToken cancellationToken)
    {
        await using var stream = await TryGetBlobAsync(blobName, null, cancellationToken);
        if (stream == null)
        {
            return default;
        }
        using var streamReader = new StreamReader(stream);
        return _jsonConvertWrapper.DeserializeObject<T>(await streamReader.ReadToEndAsync());
    }

    public async Task<bool> UploadObjectAsync<T>(T obj, string blobName, System.Threading.CancellationToken cancellationToken)
    {
        var objectString = _jsonConvertWrapper.SerializeObject(obj);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(objectString ?? ""));
        await UploadBlobInternal(stream, blobName, cancellationToken);

        return true;
    }

    public async Task DeleteBlobsByPrefix(string prefix, System.Threading.CancellationToken cancellationToken)
    {
        var blobContainerClient = await GetBlobContainerClientOrThrow();

        await foreach (var blobItem in blobContainerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
    }

    public async Task<bool> BlobExistsAsync(string blobName, IDictionary<string, string> mustMatchMetadata, System.Threading.CancellationToken cancellationToken)
    {
        var blobContainerClient = await GetBlobContainerClientOrThrow();
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        return await blobClient.ExistsAsync(cancellationToken) && (mustMatchMetadata == null || await IsMetadataMatch(blobClient, mustMatchMetadata, cancellationToken));
    }
}
