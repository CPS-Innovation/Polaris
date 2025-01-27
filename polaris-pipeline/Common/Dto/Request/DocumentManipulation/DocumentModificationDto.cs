using System.Text.Json.Serialization;

namespace Common.Dto.Request.DocumentManipulation
{
    public class DocumentModificationDto
    {
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        [JsonPropertyName("arg")]
        public object Arg { get; set; }
    }
}