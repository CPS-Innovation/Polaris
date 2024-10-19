using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Common.Dto.Tracker
{
    public class TrackerDto
    {

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public CaseRefreshStatus Status { get; set; }

        [JsonProperty("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        public List<DocumentDto> Documents { get; set; }

    }
}

