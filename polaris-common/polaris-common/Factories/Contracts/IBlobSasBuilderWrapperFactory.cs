using Azure.Storage.Sas;
using polaris_common.Wrappers.Contracts;

namespace polaris_common.Factories.Contracts;

public interface IBlobSasBuilderWrapperFactory
{
    IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder);
}
