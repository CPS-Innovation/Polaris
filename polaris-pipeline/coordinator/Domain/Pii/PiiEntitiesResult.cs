using Azure.AI.TextAnalytics;

namespace coordinator.Domain
{
    public class PiiEntitiesResult
    {
        public PiiResultEntityCollection Entities { get; set; }
        public string Id { get; set; }
        public TextDocumentStatistics Statistics { get; set; }
        public TextAnalyticsError Error { get; set; }
        public bool HasError { get; set; }
    }
}