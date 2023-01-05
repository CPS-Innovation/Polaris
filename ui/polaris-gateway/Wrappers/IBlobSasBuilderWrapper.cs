using System;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace PolarisGateway.Wrappers
{
    public interface IBlobSasBuilderWrapper
    {
        BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId);
    }
}
