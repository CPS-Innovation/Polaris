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
        public int? VersionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public CaseRefreshStatus Status { get; set; }

        [JsonProperty("running")]
        public DateTime? Running { get; set; }

        [JsonProperty("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("retrieved")]
        public float? Retrieved { get; set; }

        [JsonProperty("completed")]
        public float? Completed { get; set; }

        [JsonProperty("failed")]
        public float? Failed{ get; set; }

        [JsonProperty("failedReason")]
        public string FailedReason { get; set; }

        [JsonProperty("documents")]
        public List<DocumentDto> Documents { get; set; }
    }
}

