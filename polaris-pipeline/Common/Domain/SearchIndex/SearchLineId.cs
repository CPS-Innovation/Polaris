using Newtonsoft.Json;

namespace Common.Domain.SearchIndex
{
    public class SearchLineId : ISearchable
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}