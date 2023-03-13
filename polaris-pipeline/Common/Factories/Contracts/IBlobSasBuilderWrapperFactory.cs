using Azure.Storage.Sas;
using Common.Wrappers.Contracts;

namespace Common.Factories.Contracts;

public interface IBlobSasBuilderWrapperFactory
{
    IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder);
}
