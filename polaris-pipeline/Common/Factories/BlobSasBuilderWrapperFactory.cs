using Azure.Storage.Sas;
using Common.Wrappers;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;

namespace Common.Factories;

public class BlobSasBuilderWrapperFactory : IBlobSasBuilderWrapperFactory
{
    public IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder)
    {
        return new BlobSasBuilderWrapper(blobSasBuilder);
    }
}
