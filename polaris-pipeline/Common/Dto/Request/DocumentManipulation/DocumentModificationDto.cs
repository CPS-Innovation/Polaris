using Newtonsoft.Json;

namespace Common.Dto.Request.DocumentManipulation
{
    public class DocumentModificationDto
    {
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }
        [JsonProperty("operation")]
        public string Operation { get; set; }
        [JsonProperty("arg")]
        public object Arg { get; set; }
    }
}