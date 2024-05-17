using System.Collections.Generic;
using Newtonsoft.Json;

namespace coordinator.Services.OcrService.Domain
{
  public class AnalyzeResults
  {
    [JsonProperty(PropertyName = "version")]
    public string Version { get; set; }
    [JsonProperty(PropertyName = "modelVersion")]
    public string ModelVersion { get; set; }

    [JsonProperty(PropertyName = "readResults")]
    public IList<ReadResult> ReadResults { get; set; }
  }
}