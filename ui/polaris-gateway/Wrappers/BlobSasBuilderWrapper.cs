using System;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;

namespace PolarisGateway.Wrappers
{
    public class BlobSasBuilderWrapper : IBlobSasBuilderWrapper
    {
        private readonly BlobSasBuilder _blobSasBuilder;
        private readonly ILogger _logger;

        public BlobSasBuilderWrapper(BlobSasBuilder blobSasBuilder, ILogger logger)
        {
            _blobSasBuilder = blobSasBuilder;
            _logger = logger;
        }

        public BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(ToSasQueryParameters), string.Empty);
            
            var result = _blobSasBuilder.ToSasQueryParameters(userDelegationKey, accountName);
            _logger.LogMethodExit(correlationId, nameof(ToSasQueryParameters), string.Empty);
            return result;
        }
    }
}
