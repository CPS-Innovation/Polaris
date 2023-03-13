using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker.PresentationStatus
{
    public class PresentationStatuses
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReadStatus ReadStatus { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WriteStatus WriteStatus { get; set; }
    }
}