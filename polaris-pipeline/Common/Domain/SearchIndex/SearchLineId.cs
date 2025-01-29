using System.Text.Json.Serialization;

namespace Common.Domain.SearchIndex
{
    public class SearchLineId : ISearchable
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}