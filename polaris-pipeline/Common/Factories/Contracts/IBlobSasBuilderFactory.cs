using Azure.Storage.Sas;

namespace Common.Factories.Contracts;

public interface IBlobSasBuilderFactory
{
    BlobSasBuilder Create(string blobName);
}
