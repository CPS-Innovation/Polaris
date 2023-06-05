using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Dto.Tracker
{
    public class CaseTrackerLogsDto
    {
        [JsonProperty("case")]
        public List<TrackerCaseLogDto> Case { get; set; }

        [JsonProperty("documents")]
        public Dictionary<string, List<TrackerDocumentLogDto>> Documents { get; set; }
    }
}