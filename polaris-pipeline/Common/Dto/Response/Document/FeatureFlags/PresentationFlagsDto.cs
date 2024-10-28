using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Dto.Response.Document.FeatureFlags
{
    public class PresentationFlagsDto
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReadFlag Read { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteFlag Write { get; set; }
    }
}