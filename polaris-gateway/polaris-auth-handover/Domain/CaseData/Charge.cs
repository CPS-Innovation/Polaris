using Newtonsoft.Json;

namespace PolarisAuthHandover.Domain.CaseData
{
    public class Charge
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("isCharged")]
        public bool IsCharged { get; set; }

        [JsonProperty("nextHearingDate")]
        public string NextHearingDate { get; set; }

        [JsonProperty("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonProperty("lateDate")]
        public string LateDate { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("longDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("custodyTimeLimit")]
        public CustodyTimeLimit CustodyTimeLimit { get; set; }
    }
}
