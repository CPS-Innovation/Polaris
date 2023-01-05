using Newtonsoft.Json;

namespace RumpoleGateway.CaseDataImplementations.Cda.Domain.ResponseTypes
{
    public class ResponseCaseDetails
    {
        [JsonProperty("case")]
        public CaseDetails.CaseDetails CaseDetails { get; set; }
    }
}
