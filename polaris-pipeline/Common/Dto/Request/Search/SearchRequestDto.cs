using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Common.Dto.Request.Search
{

    public class SearchRequestDto
    {

        [JsonPropertyName("searchTerm")]
        [JsonProperty("searchTerm")]

        public string SearchTerm { get; set; }
    }
}