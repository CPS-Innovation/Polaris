using Azure;
using Azure.AI.TextAnalytics;

using Microsoft.Extensions.Configuration;

namespace Common.Services.PiiService.TextAnalytics
{
    public class TextAnalyticsClientFactory : ITextAnalyticsClientFactory
    {
        // todo: sort out where all of these keys should be
        public const string LanguageServiceUrl = nameof(LanguageServiceUrl);
        public const string LanguageServiceKey = nameof(LanguageServiceKey);

        private readonly IConfiguration _configuration;

        public TextAnalyticsClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TextAnalyticsClient Create()
        {
            return new TextAnalyticsClient(new System.Uri(_configuration[LanguageServiceUrl]), new AzureKeyCredential(_configuration[LanguageServiceKey]));
        }
    }
}