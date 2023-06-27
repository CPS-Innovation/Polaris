using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Dto.Tracker
{
    public class CaseLogsDto
    {
        [JsonProperty("case")]
        public List<CaseLogDto> Case { get; set; }

        [JsonProperty("documents")]
        public Dictionary<string, TrackerDocumentLogDto> Documents { get; set; }
    }
}