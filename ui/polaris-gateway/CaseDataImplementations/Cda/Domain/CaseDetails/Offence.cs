using Newtonsoft.Json;

namespace RumpoleGateway.CaseDataImplementations.Cda.Domain.CaseDetails
{
    public class Offence
    {
        [JsonProperty("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonProperty("lateDate")]
        public string LateDate { get; set; }

        [JsonProperty("listOrder")]
        public int ListOrder { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("longDescription")]
        public string LongDescription { get; set; }
    }
}
