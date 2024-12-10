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

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReadFlag Read { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WriteFlag Write { get; set; }
    }
}