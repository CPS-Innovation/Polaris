using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Dto.Case
{
    public class CaseDto
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
        public HeadlineChargeDto HeadlineCharge { get; set; }

        [JsonProperty("defendants")]
        public IEnumerable<DefendantDto> Defendants { get; set; }
    }
}
