using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Clients
{
	public interface IBlobStorageClient
	{
		Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, Guid correlationId);
    }
}

