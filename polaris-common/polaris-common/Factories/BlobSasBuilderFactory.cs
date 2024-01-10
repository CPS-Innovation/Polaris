using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using polaris_common.Constants;
using polaris_common.Domain.Extensions;
using polaris_common.Factories.Contracts;
using polaris_common.Logging;

namespace polaris_common.Factories;

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
            BlobContainerName = _configuration[ConfigKeys.SharedKeys.BlobServiceContainerName],
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ContentType = "application/pdf",
        };
        // todo: ContentDisposition is being set below as opposed to within the initializer above in an attempt to stop two Content-Disposition
        //  headers being set to the client.
        sasBuilder.ContentDisposition = $"inline; filename={blobName}";
        sasBuilder.ExpiresOn = sasBuilder.StartsOn.AddSeconds(double.Parse(_configuration[ConfigKeys.SharedKeys.BlobExpirySecs]));
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        _logger.LogMethodExit(correlationId, nameof(Create), sasBuilder.ToJson());
        return sasBuilder;
    }
}
