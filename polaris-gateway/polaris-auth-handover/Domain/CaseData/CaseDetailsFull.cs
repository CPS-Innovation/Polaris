using System.Collections.Generic;
using Newtonsoft.Json;

namespace PolarisAuthHandover.Domain.CaseData
{
    public class CaseDetailsFull : CaseDetails
    {
        [JsonProperty("defendants")]
        public IEnumerable<Defendant> Defendants { get; set; }
    }
}
