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
    public IList<ReadResult> ReadResults { get; set; } = new List<ReadResult>();

    [JsonIgnore]
    public long PageCount { get { return ReadResults.Count; } }

    [JsonIgnore]
    public long LineCount
    {
      get
      {
        long count = 0;
        foreach (var readResult in ReadResults)
        {
          count += readResult.Lines.Count;
        }
        return count;
      }
    }

    [JsonIgnore]
    public long WordCount
    {
      get
      {
        long count = 0;
        foreach (var readResult in ReadResults)
        {
          foreach (var line in readResult.Lines)
          {
            count += line.Words.Count;
          }
        }
        return count;
      }
    }
  }
}