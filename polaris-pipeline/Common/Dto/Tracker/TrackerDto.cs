using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Common.Dto.Tracker
{
    public class TrackerDto
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocumentDto> Documents { get; set; }

        [JsonProperty("logs")]
        public CaseTrackerLogsDto Logs { get; set; }
    }
}

