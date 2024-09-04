using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Services.BlobStorageService;

namespace pdf_thumbnail_generator.Services
{
  public static class IServiceCollectionExtension
  {
    public const string BlobServiceContainerName = "BlobServiceContainerName";
    public const string BlobServiceContainerName2 = "BlobServiceContainerName2";

    public const string BlobServiceUrl = nameof(BlobServiceUrl);

    public static void AddBlobStorageWithDefaultAzureCredentials(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddAzureClients(azureClientFactoryBuilder =>
      {
        var blobServiceUrl = GetValueFromConfig(configuration, BlobServiceUrl);
        var credentials = new DefaultAzureCredential();
        if (blobServiceUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
          azureClientFactoryBuilder.AddBlobServiceClient(new Uri(blobServiceUrl)).WithCredential(credentials);
        }
        else
        {
          azureClientFactoryBuilder.AddBlobServiceClient(blobServiceUrl).WithCredential(credentials);
        }
      });

      // Register the factory that will resolve the services
      services.AddTransient<Func<string, IPolarisBlobStorageService>>(serviceProvider => key =>
      {
        var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
        return key switch
        {
          "Documents" => new PolarisBlobStorageService(blobServiceClient, GetValueFromConfig(configuration, BlobServiceContainerName)),
          "Thumbnails" => new PolarisBlobStorageService(blobServiceClient, GetValueFromConfig(configuration, BlobServiceContainerName2)),
          _ => throw new ArgumentException($"Unknown key: {key}")
        };
      });
    }

    public static string GetValueFromConfig(IConfiguration configuration, string secretName)
    {
      var secret = configuration[secretName];
      if (string.IsNullOrWhiteSpace(secret))
      {
        throw new Exception($"Secret cannot be null: {secretName}");
      }

      return secret;
    }
  }
}