using Azure.Storage.Sas;
using text_extractor.Wrappers;

namespace text_extractor.Factories
{
    public interface IBlobSasBuilderWrapperFactory
    {
        IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder);
    }
}
