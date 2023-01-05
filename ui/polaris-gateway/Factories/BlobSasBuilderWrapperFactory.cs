using System;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Factories
{
    public class BlobSasBuilderWrapperFactory : IBlobSasBuilderWrapperFactory
    {
        private readonly ILogger<BlobSasBuilderWrapperFactory> _logger;

        public BlobSasBuilderWrapperFactory(ILogger<BlobSasBuilderWrapperFactory> logger)
        {
            _logger = logger;
        }

        public IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), string.Empty);
            
            var wrapResult = new BlobSasBuilderWrapper(blobSasBuilder, _logger);
            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return wrapResult;
        }
    }
}
