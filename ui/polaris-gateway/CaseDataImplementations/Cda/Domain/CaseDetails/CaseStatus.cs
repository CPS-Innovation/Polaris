using Newtonsoft.Json;

namespace RumpoleGateway.CaseDataImplementations.Cda.Domain.CaseDetails
{
    public class CaseStatus
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
