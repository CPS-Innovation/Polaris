using Newtonsoft.Json;
using System.Collections.Generic;

namespace PolarisGateway.CaseDataImplementations.Cda.Domain.ResponseTypes
{
    public class ResponseCaseInformationByUrn
    {
        [JsonProperty("cases")]
        public List<CaseDetails.CaseDetails> CaseDetails { get; set; }
    }
}
