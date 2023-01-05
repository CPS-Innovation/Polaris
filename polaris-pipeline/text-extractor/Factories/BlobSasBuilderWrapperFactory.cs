using Azure.Storage.Sas;
using text_extractor.Wrappers;

namespace text_extractor.Factories
{
    public class BlobSasBuilderWrapperFactory : IBlobSasBuilderWrapperFactory
    {
        public IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder)
        {
            return new BlobSasBuilderWrapper(blobSasBuilder);
        }
    }
}
