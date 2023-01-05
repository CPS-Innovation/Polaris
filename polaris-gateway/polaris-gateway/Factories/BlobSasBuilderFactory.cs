using System;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Extensions;

namespace PolarisGateway.Factories
{
    public class BlobSasBuilderFactory : IBlobSasBuilderFactory
    {
        private readonly ILogger<BlobSasBuilderFactory> _logger;
        private readonly IConfiguration _configuration;
        
        public BlobSasBuilderFactory(IConfiguration configuration, ILogger<BlobSasBuilderFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public BlobSasBuilder Create(string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), $"Blob Name: '{blobName}'");
            
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _configuration[ConfigurationKeys.BlobContainerName],
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow
            };
            sasBuilder.ExpiresOn = sasBuilder.StartsOn.AddSeconds(int.Parse(_configuration[ConfigurationKeys.BlobExpirySecs]));
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            sasBuilder.ContentType = "application/pdf";
            
            _logger.LogMethodExit(correlationId, nameof(Create), sasBuilder.ToJson());
            return sasBuilder;
        }
    }
}