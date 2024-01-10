using Azure.Storage.Sas;

namespace polaris_common.Factories.Contracts;

public interface IBlobSasBuilderFactory
{
    BlobSasBuilder Create(string blobName, Guid correlationId);
}
