using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorageService
{
    public interface IV2PolarisBlobStorageService
    {
        Task UploadDocumentAsync(Stream stream, string blobName);
        Task UploadDocumentAsync(Stream stream, string blobName, IDictionary<string, string> state);
        Task<Stream> GetDocumentAsync(string blobName);
        Task<Stream> GetDocumentAsync(string blobName, IDictionary<string, string> mustMatchMetadata);
    }
}
