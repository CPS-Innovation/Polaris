using Common.Dto.Response.Case.PreCharge;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class CaseDto
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("uniqueReferenceNumber")]
        [JsonPropertyName("uniqueReferenceNumber")]
        public string UniqueReferenceNumber { get; set; }

        [JsonProperty("isCaseCharged")]
        [JsonPropertyName("isCaseCharged")]
        public bool IsCaseCharged { get; set; }

        [JsonProperty("numberOfDefendants")]
        [JsonPropertyName("numberOfDefendants")]
        public int NumberOfDefendants { get; set; }

        [JsonProperty("owningUnit")]
        [JsonPropertyName("owningUnit")]
        public string OwningUnit { get; set; }

        [JsonProperty("leadDefendantDetails")]
        [JsonPropertyName("leadDefendantDetails")]
        public DefendantDetailsDto LeadDefendantDetails { get; set; }

        [JsonProperty("headlineCharge")]
        [JsonPropertyName("headlineCharge")]
        public HeadlineChargeDto HeadlineCharge { get; set; }

        [JsonProperty("defendants")]
        [JsonPropertyName("defendants")]
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; }

        [JsonProperty("witnesses")]
        [JsonPropertyName("witnesses")]
        public IEnumerable<WitnessDto> Witnesses { get; set; }

        [JsonProperty("preChargeDecisionRequests")]
        [JsonPropertyName("preChargeDecisionRequests")]
        public IEnumerable<PcdRequestDto> PreChargeDecisionRequests { get; set; }
    }
}
