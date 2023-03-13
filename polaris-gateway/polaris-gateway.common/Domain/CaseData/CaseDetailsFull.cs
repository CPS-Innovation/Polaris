using Newtonsoft.Json;

namespace PolarisGateway.Domain.CaseData
{
    public class CaseDetailsFull : CaseDetails
    {
        [JsonProperty("defendants")]
        public IEnumerable<Defendant> Defendants { get; set; }
    }
}
