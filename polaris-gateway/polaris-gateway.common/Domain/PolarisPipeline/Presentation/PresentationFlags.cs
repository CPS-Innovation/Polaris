using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PolarisGateway.Domain.PolarisPipeline.Presentation
{
    public class PresentationFlags
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReadFlag Read { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteFlag Write { get; set; }
    }
}