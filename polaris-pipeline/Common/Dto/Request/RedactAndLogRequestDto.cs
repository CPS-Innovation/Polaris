using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Dto.Request
{
    public class RedactAndLogRequestDto
    {
        [JsonPropertyName("redactionPayload")]
        public RedactPdfRequestDto RedactionPayload { get; set; }
        [JsonPropertyName("logPayload")]
        public CreateRedactionLogsRequest LogPayload { get; set; }
    }
}
