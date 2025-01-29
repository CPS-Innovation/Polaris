using Common.Dto.Response.Documents;
using System;
using System.Text.Json.Serialization;

namespace coordinator.Domain;

public class CaseDurableEntityState
{
    public int CaseId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("status")]
    [JsonInclude]
    public CaseRefreshStatus Status { get; set; }

    [JsonPropertyName("running")]
    [JsonInclude]
    public DateTime? Running { get; set; }

    [JsonPropertyName("Retrieved")]
    [JsonInclude]
    public float? Retrieved { get; set; }

    [JsonPropertyName("completed")]
    [JsonInclude]
    public float? Completed { get; set; }

    [JsonPropertyName("failed")]
    [JsonInclude]
    public float? Failed { get; set; }

    [JsonPropertyName("failedReason")]
    [JsonInclude]
    public string FailedReason { get; set; }
}
