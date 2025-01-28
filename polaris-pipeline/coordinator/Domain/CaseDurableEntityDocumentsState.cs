using coordinator.Durable.Payloads.Domain;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace coordinator.Domain;

public class CaseDurableEntityDocumentsState
{
    [JsonPropertyName("documents")]
    [JsonInclude]
    public List<CmsDocumentEntity> CmsDocuments { get; set; } = [];

    [JsonPropertyName("pcdRequests")]
    [JsonInclude]
    public List<PcdRequestEntity> PcdRequests { get; set; } = [];

    [JsonPropertyName("defendantsAndCharges")]
    [JsonInclude]
    public DefendantsAndChargesEntity DefendantsAndCharges { get; set; }
}
