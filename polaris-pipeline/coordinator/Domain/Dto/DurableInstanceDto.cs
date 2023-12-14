using Common.Extensions;
using Newtonsoft.Json;

namespace coordinator.Domain.Dto;

public class DurableInstanceDto
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("instanceId")]
    public string InstanceId { get; set; }
    
    [JsonProperty("runtimeStatus")]
    public string RuntimeStatus { get; set; }
    
    [JsonProperty("input")]
    public object Input { get; set; }
    
    [JsonProperty("customStatus")]
    public object CustomStatus { get; set; }
    
    [JsonProperty("output")]
    public object Output { get; set; }
    
    [JsonProperty("createdTime")]
    public string CreatedTime { get; set; }
    
    [JsonProperty("lastUpdatedTime")]
    public string LastUpdatedTime { get; set; }

    public string CaseId => InstanceId.ExtractBookendedContent("[", "]");
}
