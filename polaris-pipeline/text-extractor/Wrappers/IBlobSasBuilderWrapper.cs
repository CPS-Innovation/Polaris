using System;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace text_extractor.Wrappers
{
    public interface IBlobSasBuilderWrapper
    {
        BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId);
    }
}
