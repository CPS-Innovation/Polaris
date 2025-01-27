using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Documents
{
    public class TrackerDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public CaseRefreshStatus Status { get; set; }

        [JsonPropertyName("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

        [JsonPropertyName("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonPropertyName("documents")]
        public List<DocumentDto> Documents { get; set; }

    }
}

