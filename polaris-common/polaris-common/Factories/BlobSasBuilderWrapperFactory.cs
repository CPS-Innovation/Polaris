using Azure.Storage.Sas;
using polaris_common.Wrappers;
using polaris_common.Factories.Contracts;
using polaris_common.Wrappers.Contracts;

namespace polaris_common.Factories;

public class BlobSasBuilderWrapperFactory : IBlobSasBuilderWrapperFactory
{
    public IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder)
    {
        return new BlobSasBuilderWrapper(blobSasBuilder);
    }
}
