using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Dto.Request.Search
{

    public class SearchRequestDto
    {
        [JsonProperty("documents")]
        public List<SearchRequestDocumentDto> Documents { get; set; }

        [JsonProperty("searchTerm")]
        public string SearchTerm { get; set; }
    }
}