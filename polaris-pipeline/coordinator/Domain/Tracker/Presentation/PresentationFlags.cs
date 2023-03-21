using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker.Presentation
{
    public class PresentationFlags
    {
        public PresentationFlags()
        {
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReadFlag ReadStatus { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteFlag WriteStatus { get; set; }
    }
}