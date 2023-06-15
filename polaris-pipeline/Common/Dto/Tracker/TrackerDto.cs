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
        public float? DocumentsRetrieved { get; set; }

        [JsonProperty("pdfsGenerated")]
        public float? PdfsGenerated { get; set; }

        [JsonProperty("indexed")]
        public float? Indexed { get; set; }

        [JsonProperty("completed")]
        public float? Completed { get; set; }

        [JsonProperty("failed")]
        public float? Failed{ get; set; }

        [JsonProperty("failedReason")]
        public string FailedReason { get; set; }

        [JsonProperty("documents")]
        public List<DocumentDto> Documents { get; set; }

        [JsonProperty("logs")]
        public CaseLogsDto Logs { get; set; }
    }
}

