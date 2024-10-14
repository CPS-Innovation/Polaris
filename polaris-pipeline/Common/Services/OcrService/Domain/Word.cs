using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Services.OcrService.Domain
{
  public class Word
  {
    [JsonProperty(PropertyName = "boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "confidence")]
    public double Confidence { get; set; }
  }
}