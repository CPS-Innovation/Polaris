using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Common.Dto.Request.DocumentManipulation
{
    public class DocumentModificationDto
    {
        [JsonProperty("pageIndex")]
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("operation")]
        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        [JsonProperty("arg")]
        [JsonPropertyName("arg")]
        public object Arg { get; set; }
    }
}