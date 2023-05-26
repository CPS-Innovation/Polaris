using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Dto.Tracker
{
    public class CaseTrackerLogsDto
    {
        [JsonProperty("case")]
        public List<TrackerLogDto> Case { get; set; }

        [JsonProperty("documents")]
        public Dictionary<string, List<TrackerLogDto>> Documents { get; set; }
    }
}