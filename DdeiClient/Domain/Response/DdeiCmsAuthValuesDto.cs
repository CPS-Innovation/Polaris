using System.Text.Json.Serialization;

namespace Ddei.Domain.Response
{
    public class DdeiCmsAuthValuesDto
    {
        public string Cookies { get; set; }

        public string UserIpAddress { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PreferredLoadBalancerTarget { get; set; }

        public string Token { get; set; }
    }
}