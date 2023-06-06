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
        public CaseRefreshStatus Status { get; set; }

        [JsonProperty("running")]
        public DateTime? Running { get; set; }

        [JsonProperty("documentsRetrievedSeconds")]
        public float? DocumentsRetrievedSeconds { get; set; }

        [JsonProperty("processingCompletedSeconds")]
        public float? ProcessingCompletedSeconds { get; set; }

        [JsonProperty("failedSeconds")]
        public float? FailedSeconds { get; set; }

        [JsonProperty("logs")]
        public CaseLogsDto Logs { get; set; }

        [JsonProperty("documents")]
        public List<DocumentDto> Documents { get; set; }
    }
}

