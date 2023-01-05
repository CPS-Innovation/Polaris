using System;
using System.IO;
using System.Threading.Tasks;

namespace RumpoleGateway.Clients.RumpolePipeline
{
	public interface IBlobStorageClient
	{
		Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, Guid correlationId);
    }
}

