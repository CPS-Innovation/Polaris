using Newtonsoft.Json;
using System.Collections.Generic;

namespace PolarisGateway.Domain.CaseData
{
    public class CaseDetailsFull : CaseDetails
    {
        [JsonProperty("defendants")]
        public IEnumerable<Defendant> Defendants { get; set; }
    }
}
