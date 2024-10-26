using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Domain.Ocr
{
  public class ReadResult
  {
    [JsonProperty(PropertyName = "page")]
    public int Page { get; set; }

    [JsonProperty(PropertyName = "language")]
    public string Language { get; set; }

    [JsonProperty(PropertyName = "angle")]
    public double Angle { get; set; }

    [JsonProperty(PropertyName = "width")]
    public double Width { get; set; }

    [JsonProperty(PropertyName = "height")]
    public double Height { get; set; }

    [JsonProperty(PropertyName = "lines")]
    public IList<Line> Lines { get; set; }
  }
}