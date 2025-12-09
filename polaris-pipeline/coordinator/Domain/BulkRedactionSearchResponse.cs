using Common.Dto.Request.Redaction;
using coordinator.Enums;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace coordinator.Domain;

public class BulkRedactionSearchResponse
{
    public string Urn { get; set; }

    public int CaseId { get; set; }

    public string DocumentId { get; set; }

    public long VersionId { get; set; }

    public string SearchText { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public OrchestrationProviderStatuses DocumentRefreshStatus { get; set; }

    public IEnumerable<RedactionDefinitionDto> RedactionDefinitions { get; set; }

    public string FailedReason { get; set; }

    [JsonIgnore]
    public bool IsNotFound { get; set; }
}