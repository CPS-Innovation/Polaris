using System;
using Azure.Storage.Sas;
using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace text_extractor.Factories
{
    public class BlobSasBuilderFactory : IBlobSasBuilderFactory
    {
        private readonly IConfiguration _configuration;

        public BlobSasBuilderFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BlobSasBuilder Create(string blobName)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _configuration[ConfigKeys.SharedKeys.BlobServiceContainerName],
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow
            };
            sasBuilder.ExpiresOn = sasBuilder.StartsOn.AddSeconds(double.Parse(_configuration[ConfigKeys.SharedKeys.BlobExpirySecs]));
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return sasBuilder;
        }
    }
}