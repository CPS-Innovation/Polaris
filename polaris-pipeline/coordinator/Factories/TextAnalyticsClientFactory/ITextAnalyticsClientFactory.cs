using Azure.AI.TextAnalytics;

namespace coordinator.Factories.TextAnalyticsClientFactory
{
    public interface ITextAnalyticsClientFactory
    {
        TextAnalyticsClient Create();
    }
}