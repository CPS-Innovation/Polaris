using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using text_extractor.Factories;

namespace text_extractor.Services.SasGeneratorService
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
            _blobSasBuilderFactory = blobSasBuilderFactory;
            _blobSasBuilderWrapperFactory = blobSasBuilderWrapperFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GenerateSasUrlAsync), blobName);
            
            var now = DateTimeOffset.UtcNow;
            var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(now, 
                now.AddSeconds(double.Parse(_configuration[ConfigKeys.SharedKeys.BlobUserDelegationKeyExpirySecs])));

            var blobUri = new Uri($"{_blobServiceClient.Uri}{_configuration[ConfigKeys.SharedKeys.BlobServiceContainerName]}/{blobName}");
            var blobUriBuilder = new BlobUriBuilder(blobUri); 
            var sasBuilder = _blobSasBuilderFactory.Create(blobUriBuilder.BlobName);
            var sasBuilderWrapper = _blobSasBuilderWrapperFactory.Create(sasBuilder);        
            blobUriBuilder.Sas = sasBuilderWrapper.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName, correlationId);

            _logger.LogMethodExit(correlationId, nameof(GenerateSasUrlAsync), string.Empty);
            return blobUriBuilder.ToUri().ToString();      
        }
    }
}
