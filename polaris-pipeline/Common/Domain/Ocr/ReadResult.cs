using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Common.Domain.Ocr
{
  public class ReadResult
  {
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("angle")]
    public double Angle { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("lines")]
    public IList<Line> Lines { get; set; }
  }
}