using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Document.FeatureFlags
{
    public class PresentationFlagsDto
    {
        public PresentationFlagsDto()
        {
        }

        public PresentationFlagsDto(ReadFlag read, WriteFlag write)
        {
            Read = read;
            Write = write;
        }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public ReadFlag Read { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public WriteFlag Write { get; set; }
    }
}