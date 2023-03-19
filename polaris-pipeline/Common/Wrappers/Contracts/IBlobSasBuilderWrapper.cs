using System;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Common.Wrappers.Contracts
{
    public interface IBlobSasBuilderWrapper
    {
        BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId);
    }
}
