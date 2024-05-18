using Azure;
using Azure.AI.TextAnalytics;
using coordinator.Constants;
using Microsoft.Extensions.Configuration;

namespace coordinator.Factories.TextAnalyticsClientFactory
{
    public class TextAnalyticsClientFactory : ITextAnalyticsClientFactory
    {
        private readonly IConfiguration _configuration;

        public TextAnalyticsClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TextAnalyticsClient Create()
        {
            return new TextAnalyticsClient(new System.Uri(_configuration[ConfigKeys.LanguageServiceUrl]), new AzureKeyCredential(_configuration[ConfigKeys.LanguageServiceKey]));
        }
    }
}