using Azure.Storage.Sas;

namespace text_extractor.Factories
{
    public interface IBlobSasBuilderFactory
    {
        BlobSasBuilder Create(string blobName);
    }
}