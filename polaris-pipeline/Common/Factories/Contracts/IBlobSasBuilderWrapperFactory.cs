using Azure.Storage.Sas;
using Common.Wrappers;

namespace Common.Factories.Contracts;

public interface IBlobSasBuilderWrapperFactory
{
    IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder);
}
