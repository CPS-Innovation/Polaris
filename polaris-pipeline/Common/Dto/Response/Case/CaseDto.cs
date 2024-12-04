using Common.Dto.Response.Case.PreCharge;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class CaseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("uniqueReferenceNumber")]
        public string UniqueReferenceNumber { get; set; }

        [JsonPropertyName("isCaseCharged")]
        public bool IsCaseCharged { get; set; }

        [JsonPropertyName("numberOfDefendants")]
        public int NumberOfDefendants { get; set; }

        [JsonPropertyName("owningUnit")]
        public string OwningUnit { get; set; }

        [JsonPropertyName("leadDefendantDetails")]
        public DefendantDetailsDto LeadDefendantDetails { get; set; }

        [JsonPropertyName("headlineCharge")]
        public HeadlineChargeDto HeadlineCharge { get; set; }

        [JsonPropertyName("defendants")]
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; }

        [JsonPropertyName("witnesses")]
        public IEnumerable<WitnessDto> Witnesses { get; set; }

        [JsonPropertyName("preChargeDecisionRequests")]
        public IEnumerable<PcdRequestDto> PreChargeDecisionRequests { get; set; }
    }
}
