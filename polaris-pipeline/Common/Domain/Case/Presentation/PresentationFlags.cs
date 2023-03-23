using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Domain.Case.Presentation
{
    public class PresentationFlags
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReadFlag Read { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteFlag Write { get; set; }
    }
}