using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Domain.Ocr
{
  public class AnalyzeResults
  {
    [JsonPropertyName("version")]   
    public string Version { get; set; }
    [JsonPropertyName("modelVersion")]
    public string ModelVersion { get; set; }

    [JsonPropertyName("readResults")]
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