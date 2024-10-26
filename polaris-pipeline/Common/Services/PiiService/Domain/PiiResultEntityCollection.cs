using System.Collections.Generic;
using Azure.AI.TextAnalytics;

namespace Common.Services.PiiService.Domain
{
    public class PiiResultEntityCollection : List<PiiResultEntity>
    {
        public IList<TextAnalyticsWarning> Warnings { get; set; }
        public string RedactedText { get; set; }
    }
}