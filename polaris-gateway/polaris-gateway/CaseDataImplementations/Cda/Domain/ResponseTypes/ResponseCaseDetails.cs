using Newtonsoft.Json;

namespace PolarisGateway.CaseDataImplementations.Cda.Domain.ResponseTypes
{
    public class ResponseCaseDetails
    {
        [JsonProperty("case")]
        public CaseDetails.CaseDetails CaseDetails { get; set; }
    }
}
