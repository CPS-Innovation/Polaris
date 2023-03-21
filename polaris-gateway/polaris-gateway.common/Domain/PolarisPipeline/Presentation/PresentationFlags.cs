using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PolarisGateway.Domain.PolarisPipeline.Presentation
{
    public class PresentationFlags
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReadFlag ReadStatus { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteFlag WriteStatus { get; set; }
    }
}