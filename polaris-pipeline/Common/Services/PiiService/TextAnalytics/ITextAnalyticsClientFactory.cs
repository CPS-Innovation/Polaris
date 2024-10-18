using Azure.AI.TextAnalytics;

namespace Common.Services.PiiService.TextAnalytics
{
    public interface ITextAnalyticsClientFactory
    {
        TextAnalyticsClient Create();
    }
}