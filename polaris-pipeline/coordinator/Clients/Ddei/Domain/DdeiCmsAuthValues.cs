using Newtonsoft.Json;

namespace coordinator.Clients.Ddei.Domain
{
    public class DdeiCmsAuthValues
    {
        public string Cookies { get; set; }

        public string UserIpAddress { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredLoadBalancerTarget { get; set; }

        public string Token { get; set; }
    }
}