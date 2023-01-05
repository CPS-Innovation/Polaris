using Newtonsoft.Json;
using System.Collections.Generic;

namespace RumpoleGateway.CaseDataImplementations.Cda.Domain.CaseDetails
{
    public class CaseDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("uniqueReferenceNumber")]
        public string UniqueReferenceNumber { get; set; }

        [JsonProperty("appealType")]
        public string AppealType { get; set; }

        [JsonProperty("caseStatus")]
        public CaseStatus CaseStatus { get; set; }

        [JsonProperty("caseType")]
        public string CaseType { get; set; }

        [JsonProperty("leadDefendant")]
        public LeadDefendant LeadDefendant { get; set; }

        [JsonProperty("offences")]
        public List<Offence> Offences { get; set; }
    }
}
