using Common.Dto.Request.Redaction;
using Common.Dto.Response;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace coordinator.Domain;

public class BulkRedactionSearchEntityState
{
    public int CaseId { get; set; }
    public string DocumentId { get; set; }
    public long VersionId { get; set; }
    public string SearchTerm { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("status")]
    [JsonInclude]
    public BulkRedactionSearchStatus Status { get; set; }
    [JsonPropertyName("ocrDocumentGeneratedAt")]
    [JsonInclude]
    public DateTime? OcrDocumentGeneratedAt { get; set; }
    [JsonPropertyName("documentSearchCompletedAt")]
    [JsonInclude]
    public DateTime? DocumentSearchCompletedAt { get; set; }
    [JsonPropertyName("completedAt")]
    [JsonInclude]
    public DateTime? CompletedAt { get; set; }
    [JsonPropertyName("FailedAt")]
    [JsonInclude]
    public DateTime? FailedAt { get; set; }
    [JsonPropertyName("failureReason")]
    [JsonInclude]
    public string FailureReason { get; set; }
    [JsonPropertyName("updatedAt")]
    [JsonInclude]
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<RedactionDefinitionDto> RedactionDefinitions { get; set; }
}
