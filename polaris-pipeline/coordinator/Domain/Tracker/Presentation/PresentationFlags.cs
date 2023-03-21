using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker.Presentation
{
    public class PresentationFlags
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReadFlag ReadStatus { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteFlag WriteStatus { get; set; }
    }
}