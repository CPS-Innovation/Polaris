﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Documents
{
    public class TrackerDto
    {

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public CaseRefreshStatus Status { get; set; }

        [JsonProperty("documentsRetrieved")]
        [JsonPropertyName("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

        [JsonProperty("processingCompleted")]
        [JsonPropertyName("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        [JsonPropertyName("documents")]
        public List<DocumentDto> Documents { get; set; }

    }
}

