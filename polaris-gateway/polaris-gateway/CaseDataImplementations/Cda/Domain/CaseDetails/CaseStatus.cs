using Newtonsoft.Json;

namespace PolarisGateway.CaseDataImplementations.Cda.Domain.CaseDetails
{
    public class CaseStatus
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
