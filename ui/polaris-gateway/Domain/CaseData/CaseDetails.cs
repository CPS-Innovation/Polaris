using Newtonsoft.Json;

namespace PolarisGateway.Domain.CaseData
{
    public class CaseDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("uniqueReferenceNumber")]
        public string UniqueReferenceNumber { get; set; }

        [JsonProperty("isCaseCharged")]
        public bool IsCaseCharged { get; set; }

        [JsonProperty("numberOfDefendants")]
        public int NumberOfDefendants { get; set; }

        [JsonProperty("leadDefendantDetails")]
        public DefendantDetails LeadDefendantDetails { get; set; }

        [JsonProperty("headlineCharge")]
        public HeadlineCharge HeadlineCharge { get; set; }
    }
}
