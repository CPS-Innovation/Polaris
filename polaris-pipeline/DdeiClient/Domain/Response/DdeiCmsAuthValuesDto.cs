using Newtonsoft.Json;

namespace Ddei.Domain.Response
{
    public class DdeiCmsAuthValuesDto
    {
        public string Cookies { get; set; }

        public string UserIpAddress { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredLoadBalancerTarget { get; set; }

        public string Token { get; set; }
    }
}