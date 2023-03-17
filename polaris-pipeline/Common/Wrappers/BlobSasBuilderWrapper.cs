using System;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Common.Wrappers.Contracts;

namespace Common.Wrappers
{
    public class BlobSasBuilderWrapper : IBlobSasBuilderWrapper
    {
        private readonly BlobSasBuilder _blobSasBuilder;
        
        public BlobSasBuilderWrapper(BlobSasBuilder blobSasBuilder)
        {
            _blobSasBuilder = blobSasBuilder;
        }

        public BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId)
        {
            var result = _blobSasBuilder.ToSasQueryParameters(userDelegationKey, accountName);
            return result;
        }
    }
}
