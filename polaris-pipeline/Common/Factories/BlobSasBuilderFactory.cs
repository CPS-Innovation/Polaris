using System;
using Azure.Storage.Sas;
using Common.Constants;
using Common.Factories.Contracts;
using Microsoft.Extensions.Configuration;

namespace Common.Factories;

public class BlobSasBuilderFactory : IBlobSasBuilderFactory
{
    private readonly IConfiguration _configuration;

    public BlobSasBuilderFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public BlobSasBuilder Create(string blobName, Guid correlationId)
    {
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

        return sasBuilder;
    }
}
