using Newtonsoft.Json;

namespace Common.Dto.Request.Search
{

    public class SearchRequestDto
    {

        [JsonProperty("searchTerm")]
        public string SearchTerm { get; set; }
    }
}