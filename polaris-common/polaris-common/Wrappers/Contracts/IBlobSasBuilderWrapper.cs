using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace polaris_common.Wrappers.Contracts
{
    public interface IBlobSasBuilderWrapper
    {
        BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId);
    }
}
