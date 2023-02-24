using System;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Clients.PolarisPipeline
{
	public interface IBlobStorageClient
	{
		[Obsolete("Moving to Polaris Common library")]
		Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        [Obsolete("Moving to Polaris Common library")]
        Task UploadDocumentAsync(Stream stream, string blobName, Guid correlationId);
    }
}

