using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Factories;

namespace RumpoleGateway.Services
{
    public class SasGeneratorService : ISasGeneratorService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IBlobSasBuilderFactory _blobSasBuilderFactory;
        private readonly IBlobSasBuilderWrapperFactory _blobSasBuilderWrapperFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SasGeneratorService> _logger;

        public SasGeneratorService(
            BlobServiceClient blobServiceClient,
            IBlobSasBuilderFactory blobSasBuilderFactory,
            IBlobSasBuilderWrapperFactory blobSasBuilderWrapperFactory,
            IConfiguration configuration, 
            ILogger<SasGeneratorService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _blobSasBuilderFactory = blobSasBuilderFactory ?? throw new ArgumentNullException(nameof(blobSasBuilderFactory));
            _blobSasBuilderWrapperFactory = blobSasBuilderWrapperFactory ?? throw new ArgumentNullException(nameof(blobSasBuilderWrapperFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GenerateSasUrlAsync), $"For blob name: '{blobName}'");
            
            var now = DateTimeOffset.UtcNow;
            var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(now, now.AddSeconds(int.Parse(_configuration[ConfigurationKeys.BlobUserDelegationKeyExpirySecs])));

            var blobUri = new Uri($"{_blobServiceClient.Uri}{_configuration[ConfigurationKeys.BlobContainerName]}/{blobName}");
            var blobUriBuilder = new BlobUriBuilder(blobUri); 
            var sasBuilder = _blobSasBuilderFactory.Create(blobUriBuilder.BlobName, correlationId);
            var sasBuilderWrapper = _blobSasBuilderWrapperFactory.Create(sasBuilder, correlationId);        
            blobUriBuilder.Sas = sasBuilderWrapper.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName, correlationId);

            _logger.LogMethodEntry(correlationId, nameof(GenerateSasUrlAsync), string.Empty);
            return blobUriBuilder.ToUri().ToString();      
        }
    }
}
